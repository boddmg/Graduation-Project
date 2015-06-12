#!/usr/bin/env python
import sys
import os
sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))

from blocks.bricks import Linear, Rectifier, Softmax, Sigmoid, Tanh, MLP, Maxout, Sequence, Identity
from blocks.bricks.recurrent import SimpleRecurrent
from blocks.bricks.conv import ConvolutionalLayer, ConvolutionalSequence, Flattener
from blocks import initialization

from theano import tensor
import theano
from blocks.bricks.cost import CategoricalCrossEntropy, MisclassificationRate
from blocks.graph import ComputationGraph
from blocks.graph import apply_dropout
from blocks.initialization import IsotropicGaussian, Constant, Uniform
from Preprocessor.Base_utils import *
from Preprocessor.dataset_utils import PackerForFuel
from fuel.streams import DataStream
from fuel.schemes import SequentialScheme, ShuffledScheme
from blocks.algorithms import GradientDescent, Scale, CompositeRule, VariableClipping
from blocks.extensions.monitoring import DataStreamMonitoring
from blocks.extensions.monitoring import TrainingDataMonitoring
from blocks.extensions.plot import Plot
from blocks.main_loop import MainLoop
from blocks.extensions import FinishAfter, Printing, Timing
from blocks.filter import VariableFilter
from blocks.roles import INPUT, OUTPUT

sys.stdout = os.fdopen(sys.stdout.fileno(), 'w', 0)


def main():
    ## Init the params.
    BATCH_SIZE = 256
    FRAME_NUM = 48
    FRAME_SIZE = 144
    IMAGE_SIZE = [FRAME_NUM, FRAME_SIZE]

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
        conv_layers.append(ConvolutionalLayer(activation, filter_size, num_filters_, pooling_size))

    convnet = ConvolutionalSequence(conv_layers, num_channels=1,
                                    image_size=tuple(IMAGE_SIZE),
                                    weights_init=IsotropicGaussian(),
                                    biases_init=Constant(0.))
    convnet.initialize()

    # Fully connected layers

    features = Flattener().apply(convnet.apply(x))
    # features = x.flatten()
    linear1 = Linear(320, 200 , weights_init=IsotropicGaussian(),
                                  biases_init=Constant(0.))
    linear1.initialize()

    linear2 = Linear(200, 100, weights_init=IsotropicGaussian(),
                                  biases_init=Constant(0.))
    linear2.initialize()

    linear3 = Linear(100, 14,  weights_init=IsotropicGaussian(),
                                  biases_init=Constant(0.))
    linear3.initialize()

    rnn = SimpleRecurrent(dim=2, activation=Identity(), weights_init=initialization.Identity())
    rnn.initialize()

    rnn_linear1 = MLP(activations=[Sigmoid()],
              dims=[320, 256], weights_init=IsotropicGaussian(),
              biases_init=Constant(0.))
    rnn_linear1.initialize()

    rnn_linear2 = MLP(activations=[Sigmoid()],
              dims=[256, 100], weights_init=IsotropicGaussian(),
              biases_init=Constant(0.))
    rnn_linear2.initialize()

    rnn_linear3 = MLP(activations=[Softmax()],
              dims=[100, 14], weights_init=IsotropicGaussian(),
              biases_init=Constant(0.))
    rnn_linear3.initialize()

    probs = rnn.apply(rnn_linear1.apply(features))
    probs = rnn_linear2.apply(probs)
    probs = rnn_linear3.apply(probs)

    cost = CategoricalCrossEntropy().apply(y.flatten(), probs)
    correct_rate = 1 - MisclassificationRate().apply(y.flatten(), probs)
    correct_rate.name = 'correct_rate'
    cost.name = 'cost'

    cg = ComputationGraph(cost)

    ## Carve the data into lots of batches.

    data_stream_train = DataStream(cad60_train, iteration_scheme = ShuffledScheme(
        cad60_train.num_examples, batch_size = BATCH_SIZE))

    ## Set the algorithm for the training.
    algorithm = GradientDescent(cost = cost, params = cg.parameters,
                                # step_rule = CompositeRule([VariableClipping(50), Scale(0.1)]) )
                                step_rule = CompositeRule([Scale(0.1)]))

    ## Add a monitor extension for the training.
    data_stream_test = DataStream(cad60_test, iteration_scheme = ShuffledScheme(
        cad60_test.num_examples, batch_size = BATCH_SIZE))

    test_monitor = DataStreamMonitoring(variables = [cost, correct_rate], data_stream = data_stream_test,
                                   prefix = "test" )

    train_monitor = TrainingDataMonitoring(variables = [cost, correct_rate, algorithm.total_step_norm],
                                           prefix = 'train', after_batch=True)


    # Add a plot monitor.
    plot = Plot(document = 'new',
                channels=[['test_correct_rate', 'train_correct_rate']],
                start_server = True)

    print("Start training")
    main_loop = MainLoop(algorithm=algorithm, data_stream=data_stream_train,
                         extensions=[Timing(),
                                     test_monitor,
                                     train_monitor,
                                     FinishAfter(after_n_epochs = 150),
                                     Printing(),
                                     plot
                                     ])
    main_loop.run()

if __name__ == "__main__":
    main()

