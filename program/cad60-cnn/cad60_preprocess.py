#!/usr/bin/env python
__author__ = 'boddmg'


from cad60_skeleton import CAD60
from Preprocessor.Base_utils import *
from pylearn2.models.autoencoder import Autoencoder, DenoisingAutoencoder
from pylearn2.models.rbm import GaussianBinaryRBM
from pylearn2.corruption import BinomialCorruptor
from pylearn2.corruption import GaussianCorruptor
from pylearn2.energy_functions.rbm_energy import GRBM_Type_1
from pylearn2.training_algorithms.sgd import SGD
from pylearn2.costs.autoencoder import MeanSquaredReconstructionError
from pylearn2.termination_criteria import EpochCounter
from pylearn2.costs.ebm_estimation import SMD
from pylearn2.training_algorithms.sgd import MonitorBasedLRAdjuster
from pylearn2.train import Train



MAX_EPOCHS_UNSUPERVISED = 1
MAX_EPOCHS_SUPERVISED = 2

class CAD60Loader(Preprocessor):
    def __init__(self, data_type = "train"):
        self.data_type = data_type
        pass

    def run(self, src_data = None, src_labels = None):
        src_data, src_labels, temp = CAD60(batch_size=1, data_type=self.data_type).get_data()
        return src_data, src_labels

def get_autoencoder(structure):
    n_input, n_output = structure
    config = {
        'nhid': n_output,
        'nvis': n_input,
        'tied_weights': True,
        'act_enc': 'sigmoid',
        'act_dec': 'sigmoid',
        # 'act_enc': 'tanh',
        # 'act_dec': 'tanh',
        'irange': 0.1,
    }
    return Autoencoder(**config)

def get_denoising_autoencoder(structure):
    n_input, n_output = structure
    curruptor = BinomialCorruptor(corruption_level=0.5)
    config = {
        'corruptor': curruptor,
        'nhid': n_output,
        'nvis': n_input,
        'tied_weights': True,
        'act_enc': 'sigmoid',
        'act_dec': 'sigmoid',
        'irange': 0.001,
    }
    return DenoisingAutoencoder(**config)

def get_grbm(structure):
    n_input, n_output = structure
    config = {
        'nvis': n_input,
        'nhid': n_output,
        "irange" : 0.05,
        "energy_function_class" : GRBM_Type_1,
        "learn_sigma" : True,
        "init_sigma" : .4,
        "init_bias_hid" : -2.,
        "mean_vis" : False,
        "sigma_lr_scale" : 1e-3
        }

    return GaussianBinaryRBM(**config)

def get_layer_trainer_sgd_autoencoder(layer, trainset):
    # configs on sgd
    train_algorithm = SGD(
            learning_rate = 0.1,
              cost =  MeanSquaredReconstructionError(),
              batch_size =  10,
              monitoring_batches = 10,
              monitoring_dataset =  trainset,
              termination_criterion = EpochCounter(max_epochs=MAX_EPOCHS_UNSUPERVISED),
              update_callbacks =  None
              )

    model = layer
    extensions = None
    return Train(model = model,
            algorithm = train_algorithm,
            extensions = extensions,
            dataset = trainset)

def get_layer_trainer_sgd_rbm(layer, trainset):
    train_algorithm = SGD(
        learning_rate = 1e-1,
        batch_size =  5,
        #"batches_per_iter" : 2000,
        monitoring_batches =  20,
        monitoring_dataset =  trainset,
        cost = SMD(corruptor=GaussianCorruptor(stdev=0.4)),
        termination_criterion =  EpochCounter(max_epochs=MAX_EPOCHS_UNSUPERVISED),
        )
    model = layer
    extensions = [MonitorBasedLRAdjuster()]
    return Train(model = model, algorithm = train_algorithm,
                 save_path='grbm.pkl',save_freq=1,
                 extensions = extensions, dataset = trainset)

def main():
    src_data, src_labels = PreprocessorList([
        DataLoad("cad60_train.hkl"),
        Encoder(get_layer_trainer_sgd_rbm,get_grbm([170,30])),
        DataDump("cad60_train_feature.hkl")]).run()
    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()
