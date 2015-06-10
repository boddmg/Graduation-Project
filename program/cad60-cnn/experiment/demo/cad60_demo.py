#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))

from blocks.bricks import Softmax, Sigmoid, MLP
from blocks.bricks.conv import ConvolutionalLayer, ConvolutionalSequence, Flattener

from theano import tensor
from blocks.initialization import IsotropicGaussian, Constant
from Preprocessor.dataset_utils import PackerForFuel
from Preprocessor.Base_utils import *
from utilities import *
import numpy as np

sys.stdout = os.fdopen(sys.stdout.fileno(), 'w', 0)

## Init the params.
FRAME_NUM = 48
FRAME_SIZE = 144
IMAGE_SIZE = [FRAME_NUM, FRAME_SIZE]

def prepare_data():
    # Prepare the data.
    print("Prepare the data.")
    data, label = PreprocessorList([
            DataLoad("cad60_test_feature.hkl"),
            SplitIntoBatches(FRAME_NUM,5),
            Monitor()]).run()
    cad60_test = PackerForFuel(data, label)

    data, label = PreprocessorList([
            DataLoad("cad60_train_feature.hkl"),
            SplitIntoBatches(FRAME_NUM,5),
            Monitor()]).run()
    cad60_train = PackerForFuel(data, label)
    return cad60_train, cad60_test

class Detector():
    def __init__(self):
        # Build the network.
        print("Build the network")
        self.x = tensor.tensor3('features')

        self.x = self.x.reshape((self.x.shape[0], 1, IMAGE_SIZE[0], IMAGE_SIZE[1]))
        # x = x.reshape((x.shape[0], 1, 1, x.shape[2]))

        y = tensor.lmatrix('targets')

        # Convolutional layers
        filter_sizes = [(3, 3)] * 3
        num_filters = [16, 32, 64]
        pooling_sizes = [(3, 3)] * 3
        activation = Sigmoid().apply
        conv_layers = []

        input_dims = list(IMAGE_SIZE)
        for filter_size, num_filters_, pooling_size in zip(filter_sizes, num_filters, pooling_sizes):
            try:
                layer_count = layer_count+1
            except:
                layer_count = 0
            new_conv_layer = ConvolutionalLayer(activation, filter_size, num_filters_, pooling_size)
            new_conv_layer.name += str(layer_count)
            conv_layers.append(new_conv_layer)

        convnet = ConvolutionalSequence(conv_layers, num_channels=1,
                                        image_size=tuple(IMAGE_SIZE),
                                        weights_init=IsotropicGaussian(),
                                        biases_init=Constant(0.))
        convnet.initialize()

        # Fully connected layers

        features = Flattener().apply(convnet.apply(self.x))
        mlp = MLP(activations=[Sigmoid(), Sigmoid(), Softmax()],
                  dims=[320, 256, 100, 14], weights_init=IsotropicGaussian(),
                  biases_init=Constant(0.))
        mlp.initialize()

        probs = mlp.apply(features)
        load_params(probs, "params-after-train.pkl")
        self.model = probs

    def detect(self, data):
        suit_shape_of_x = lambda x:x.reshape(1,1,IMAGE_SIZE[0],IMAGE_SIZE[1])
        result = self.model.eval({self.x:suit_shape_of_x(data)})[0]
        result = list(result)
        return result.index(max(result))

norm_param = {}
CONFIDENCE_INDEX = [9, 13, 23, 27, 37, 41, 51, 55, 65, 69, 79, 83, 93, 97, 107, 111, 121, 125, 135, 139,
                  149, 153, 157, 161, 165, 169]
POSE_INDEX = list(set(range(170)) - set(CONFIDENCE_INDEX))
POSE_INDEX.sort()


def norm(data):
    if norm_param == {}:
        PreprocessorList([
            DataLoad("../../cad60_train.hkl"),
            Index(POSE_INDEX),
            Normalization(norm_param, False),
        ]).run()
    d,l = Normalization(norm_param, True).run(data, None)
    return d



def main():
    import random
    d = Detector()
    d.detect(norm(np.array([[random.random()]*144]*48, dtype=np.float32)))

    history = []

    import zmq
    context = zmq.Context()
    pull_data_socket = context.socket(zmq.PULL)
    pull_data_socket.bind("tcp://*:23333")

    push_result_socket = context.socket(zmq.PUSH)
    push_result_socket.bind("tcp://*:23334")
    print("begin to receive!")
    while True:
        new_data = pull_data_socket.recv()
        new_data = map(float,new_data.split(","))
        history += [new_data]
        print(len(history))
        if len(history)>48:
            history.pop(0)
            history_np = np.array(history, dtype=np.float32)
            history_np = norm(history_np)
            result = str(d.detect(history_np))
            push_result_socket.send(result)
        else:
            push_result_socket.send("-1")

def test():
    import zmq
    context = zmq.Context()
    pull_data_socket = context.socket(zmq.PULL)
    pull_data_socket.bind("tcp://*:23333")

    push_result_socket = context.socket(zmq.PUSH)
    push_result_socket.bind("tcp://*:23334")
    print("begin to receive!")
    cnt = 0
    while True:
        cnt+=1
        new_data = pull_data_socket.recv()
        push_result_socket.send(str(cnt))


if __name__ == "__main__":
    main()

