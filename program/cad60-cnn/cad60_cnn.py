#!/usr/bin/env python

from blocks.bricks import Linear, Rectifier, Softmax, Sigmoid, Tanh, MLP
from blocks.bricks.conv import ConvolutionalLayer, ConvolutionalSequence, Flattener

from theano import tensor
from blocks.bricks.cost import CategoricalCrossEntropy, MisclassificationRate
from blocks.graph import ComputationGraph
from blocks.initialization import IsotropicGaussian, Constant, Uniform
from cad60_skeleton import CAD60Skeleton
from fuel.streams import DataStream
from fuel.schemes import SequentialScheme
from blocks.algorithms import GradientDescent, Scale
from blocks.extensions.monitoring import DataStreamMonitoring
from blocks.extensions.monitoring import TrainingDataMonitoring
from blocks.extensions.plot import Plot
from blocks.main_loop import MainLoop
from blocks.extensions import FinishAfter, Printing, Timing


def main():
    BATCH_SIZE = 256
    FRAME_NUM = 50
    IMAGE_SIZE = [FRAME_NUM, 170]
    print("Build the network")
    x = tensor.tensor3('features')

    x = x.reshape((x.shape[0], 1, IMAGE_SIZE[0], IMAGE_SIZE[1]))

    y = tensor.lmatrix('targets')

    # Convolutional layers
    filter_sizes = [(5, 8)] * 2
    num_filters = [32, 32]
    pooling_sizes = [(2, 3)]*2
    activation = Sigmoid().apply
    conv_layers = []

    input_dims = list(IMAGE_SIZE)
    for filter_size, num_filters_, pooling_size in zip(filter_sizes, num_filters, pooling_sizes):
        conv_layers.append(ConvolutionalLayer(activation, filter_size, num_filters_, pooling_size))

    convnet = ConvolutionalSequence(conv_layers, num_channels=1,
                                    image_size=tuple(IMAGE_SIZE),
                                    weights_init=Uniform(0, 0.2),
                                    biases_init=Constant(0.))
    convnet.initialize()



    # Fully connected layers

    features = Flattener().apply(convnet.apply(x))
    mlp = MLP(activations=[Softmax()],
              dims=[5120, 14], weights_init=Uniform(0, 1),
              biases_init=Constant(0.))
    mlp.initialize()
    probs = mlp.apply(features)


    cost = CategoricalCrossEntropy().apply(y.flatten(), probs)
    correct_rate =  1 - MisclassificationRate().apply(y.flatten(), probs)
    correct_rate.name = 'correct_rate'
    cost.name = 'cost'

    cg = ComputationGraph(cost)

    # Train
    print("Prepare the data.")
    cad60_train = CAD60Skeleton("train", FRAME_NUM)
    cad60_test = CAD60Skeleton("test", FRAME_NUM)

    ## Carve the data into lots of batches.

    data_stream_train = DataStream(cad60_train, iteration_scheme=SequentialScheme(
        cad60_train.num_examples, batch_size = BATCH_SIZE))

    ## Set the algorithm for the training.
    algorithm = GradientDescent(cost = cost, params = cg.parameters,
                                step_rule = Scale(0.1) )

    ## Add a monitor extension for the training.
    data_stream_test = DataStream(cad60_test, iteration_scheme = SequentialScheme(
        cad60_test.num_examples, batch_size = BATCH_SIZE))

    test_monitor = DataStreamMonitoring(variables = [cost, correct_rate], data_stream = data_stream_test,
                                   prefix = "test" , after_every_epoch = True)

    train_monitor = TrainingDataMonitoring(variables = [cost, correct_rate, algorithm.total_step_norm],
                                           prefix = 'train',
                                           after_every_batch = True)

    ## Add a plot monitor.
    plot = Plot(document = 'new',
                channels=[['train_correct_rate','test_correct_rate']],
                start_server = True,
                after_every_batch = True)

    print("Start training")
    main_loop = MainLoop(algorithm=algorithm, data_stream=data_stream_train,
                         extensions=[Timing(),
                                     # plot,
                                     test_monitor,
                                     train_monitor,
                                     FinishAfter(after_n_epochs = 2),
                                     Printing()
                                     ])
    main_loop.run()

if __name__ == "__main__":
    main()

