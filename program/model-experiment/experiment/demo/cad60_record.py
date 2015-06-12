#!/usr/bin/env python
import sys
import os
import signal

sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))

from blocks.bricks import Softmax, Sigmoid, MLP
from blocks.bricks.conv import ConvolutionalLayer, ConvolutionalSequence, Flattener

from theano import tensor
from blocks.initialization import IsotropicGaussian, Constant
from Preprocessor.dataset_utils import PackerForFuel
from Preprocessor.Base_utils import *
from utilities import *
import numpy as np
import hickle

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


history = []

def exit_func(signal, frame):
    print("start to dump........")
    history_np = np.array(history, dtype=np.float32)
    print("Dumping to %s........" % sys.argv[1])
    history_np.dump(open(sys.argv[1],"w"))
    print("dump finished........")
    print("Exiting")
    sys.exit(0)

def main():
    global history
    if len(sys.argv) < 2:
        print("You must give a file name to save the data.")
        sys.exit(0)
    else:
        print("The file is %s" % sys.argv[1])
    frames_counter = 0

    import zmq
    context = zmq.Context()
    pull_data_socket = context.socket(zmq.PULL)
    pull_data_socket.bind("tcp://*:23333")

    push_result_socket = context.socket(zmq.PUSH)
    push_result_socket.bind("tcp://*:23334")

    signal.signal(signal.SIGINT, exit_func)
    print("begin to receive!")
    while True:
        new_data = pull_data_socket.recv()
        new_data = map(float, new_data.split(","))
        history += [new_data]
        frames_counter += 1
        print frames_counter
        push_result_socket.send(str(frames_counter))

if __name__ == "__main__":
    main()

