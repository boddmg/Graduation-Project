#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))

from blocks.bricks import Softmax, Sigmoid, MLP
from blocks.bricks.conv import ConvolutionalLayer, ConvolutionalSequence, Flattener

from theano import tensor
from blocks.initialization import IsotropicGaussian, Constant

sys.stdout = os.fdopen(sys.stdout.fileno(), 'w', 0)

## Init the params.
FRAME_NUM = 48
FRAME_SIZE = 144
IMAGE_SIZE = [FRAME_NUM, FRAME_SIZE]

def prepare_data():
    from Preprocessor.dataset_utils import PackerForFuel
    from Preprocessor.Base_utils import *
    from utilities import *

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
    return

class detector():
    def __init__(self):
        # Build the network.
        print("Build the network")
        x = tensor.tensor3('features')

        x = x.reshape((x.shape[0], 1, IMAGE_SIZE[0], IMAGE_SIZE[1]))
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

        features = Flattener().apply(convnet.apply(x))
        mlp = MLP(activations=[Sigmoid(), Sigmoid(), Softmax()],
                  dims=[320, 256, 100, 14], weights_init=IsotropicGaussian(),
                  biases_init=Constant(0.))
        mlp.initialize()

        probs = mlp.apply(features)
        load_params(probs, "params-after-train.pkl")
        self.model = probs

    def detect(self, data):
        suit_shape_of_x = lambda x:x.reshape(1,1,IMAGE_SIZE[0],IMAGE_SIZE[1])
        result = self.model.eval({x:suit_shape_of_x(data)})[0]
        result = list(result)
        print result
        return result.index(max(result))



def test():
    import zmq
    import uuid
    client = {}
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind("tcp://*:5555")
    print("begin to receive!")
    while True:
        new_data = socket.recv_pyobj()
        for i in new_data:
            try:
                client[i] += [new_data[i]]
            except:
                client[i] = [] + [new_data[i]]
            if len(client[i]) >=48 :
                print len(client)
        socket.send("ok!")




if __name__ == "__main__":
    test()

