#!/usr/bin/env python
import pylearn2.train
import pylearn2.datasets.mnist
import pylearn2.models.mlp
import pylearn2.space
import pylearn2.models.mlp
import pylearn2.training_algorithms.sgd
import pylearn2.training_algorithms.learning_rule
import pylearn2.costs.cost
import pylearn2.costs.mlp
import pylearn2.termination_criteria
import pylearn2.train_extensions.best_params
import cad60_dataset4pylearn2
from utils import print_mem

batch_size = 128
frame_num = 50
frame_size = 170
input_shape = [frame_num, frame_size]
output_channels_h2 = 32
output_channels_h3 = 64
max_epochs = 500
save_path = '.'

train_dataset = cad60_dataset4pylearn2.CAD60Skeleton(
    set_type='train',
    batch_size=frame_num)

test_dataset = cad60_dataset4pylearn2.CAD60Skeleton(
    set_type='test',
    batch_size=frame_num)

train = pylearn2.train.Train(
    dataset = train_dataset,
    model = pylearn2.models.mlp.MLP(
        batch_size = batch_size,
        input_space = pylearn2.space.Conv2DSpace(
            shape = input_shape,
            num_channels = 1
        ),
        layers = [
            pylearn2.models.mlp.ConvElemwise(
                layer_name = 'h2',
                output_channels = output_channels_h2,
                irange = 0.05,
                kernel_shape = [10, 8],
                pool_shape = [3, 10],
                pool_stride = [3, 10],
                max_kernel_norm = 1.9365,
                nonlinearity=pylearn2.models.mlp.SigmoidConvNonlinearity()
            ),
            pylearn2.models.mlp.ConvElemwise(
                layer_name = 'h3',
                output_channels = output_channels_h3,
                irange = 0.05,
                kernel_shape = [10, 8],
                pool_shape = [3, 10],
                pool_stride = [3, 10],
                max_kernel_norm = 1.9365,
                nonlinearity=pylearn2.models.mlp.SigmoidConvNonlinearity()
            ),
            pylearn2.models.mlp.Softmax(
                max_col_norm = 1.9365,
                layer_name='y',
                n_classes=train_dataset.y_labels,
                istdev= 0.05
            )
        ]
    ),
    algorithm=pylearn2.training_algorithms.sgd.SGD(
        batch_size=batch_size,
        train_iteration_mode='even_sequential',
        monitor_iteration_mode='even_sequential',
        learning_rate=0.01,
        learning_rule=pylearn2.training_algorithms.learning_rule.Momentum(
            init_momentum=0.5
        ),
        monitoring_dataset={
            'test':test_dataset
        },
        cost=pylearn2.costs.cost.SumOfCosts(
            costs=[
                pylearn2.costs.cost.MethodCost(
                    method='cost_from_X'
                ),
                pylearn2.costs.mlp.WeightDecay(
                    coeffs=[0.00005, 0.00005, 0.00005]
                )
            ]
        ),
        termination_criterion=pylearn2.termination_criteria.And(
            criteria=[
                pylearn2.termination_criteria.MonitorBased(
                    channel_name="test_y_misclass",
                    prop_decrease=0.50,
                    N=10
                ),
                pylearn2.termination_criteria.EpochCounter(
                    max_epochs=max_epochs
                )
            ]
        )
    ),
    extensions=[
        pylearn2.train_extensions.best_params.MonitorBasedSaveBest(
            channel_name='test_y_misclass',
            save_path=save_path+'/convolutional_network_best.pkl'
        ),
        pylearn2.training_algorithms.learning_rule.MomentumAdjustor(
            start=1,
            saturate=10,
            final_momentum=0.99
        )
    ]
)

train.main_loop()
