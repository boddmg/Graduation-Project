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
        # push_result_socket.send(str(frames_counter))
        push_result_socket.send("1")

if __name__ == "__main__":
    main()

