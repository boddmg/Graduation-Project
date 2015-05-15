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
from Base_utils import Preprocessor
from dataset_utils import Dataset
from dataset_utils import datasetformat_2_src
from dataset_utils import src_2_datasetformat
from pylearn2.utils import serial
from dataset_utils import Dataset
from theano import tensor
import theano
from rbm import *


def get_autoencoder(structure):
    n_input, n_output = structure
    config = {
        'nvis': n_input,
        'nhid': n_output,
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
        'nvis': n_input,
        'nhid': n_output,
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

def get_layer_trainer_sgd_autoencoder(layer, trainset, max_epoches, save_path):
    # configs on sgd
    train_algorithm = SGD(
            learning_rate = 0.1,
              cost =  MeanSquaredReconstructionError(),
              batch_size =  10,
              monitoring_batches = 10,
              monitoring_dataset =  trainset,
              termination_criterion = EpochCounter(max_epochs= max_epoches),
              update_callbacks =  None
              )

    model = layer
    extensions = None
    return Train(model = model,
                 allow_overwrite = True,
                 save_path = save_path,
                 algorithm = train_algorithm,
                 extensions = extensions,
                 dataset = trainset)

def get_layer_trainer_sgd_rbm(layer, trainset, max_epoches, save_path):
    train_algorithm = SGD(
        learning_rate = 1e-1,
        batch_size =  20,
        #"batches_per_iter" : 2000,
        monitoring_batches =  20,
        monitoring_dataset =  trainset,
        cost = SMD(corruptor=GaussianCorruptor(stdev=0.4)),
        termination_criterion =  EpochCounter(max_epochs= max_epoches),
        )
    model = layer
    extensions = [MonitorBasedLRAdjuster()]
    return Train(model = model,
                 algorithm = train_algorithm,
                 save_path= save_path,
                 extensions = extensions,
                 dataset = trainset,
                 allow_overwrite = True)



class Encoder(Preprocessor):
    def __init__(self, trainer_generator, layer, max_epoches = 1, save_path = None, load_path = None):
        self.trainer_generator = trainer_generator
        self.layer = layer
        self.max_epoches = max_epoches
        self.save_path = save_path
        self.load_path = load_path

    def run(self,src_data, src_labels):
        src_data = src_2_datasetformat(src_data)

        dataset = Dataset(src_data, src_labels[:, None])
        if self.load_path == None:
            self.trainer_generator(self.layer, dataset, self.max_epoches, None).main_loop() # self.save_path
            serial.save(self.save_path, self.layer)
        else:
            self.layer = serial.load(self.load_path)
        output = self.layer.perform(dataset.get_design_matrix())
        output = datasetformat_2_src(output)
        return output, src_labels

class mock_encoder(Preprocessor):
    def __init__(self):
        pass
    def run(self, src_data, src_labels):
        src_data = src_2_datasetformat(src_data)
        dataset = Dataset(src_data, src_labels[:, None])
        output = dataset.get_design_matrix()
        output = datasetformat_2_src(output)
        return output, src_labels

class pca_encoder(Preprocessor):
    def __init__(self, in_out_structure, pca = None, train = True):
        self.structure = in_out_structure
        self.pca = pca
        self.train = train

    def run(self, src_data, src_labels):
        dst_data = src_2_datasetformat(src_data)
        dataset = Dataset(dst_data, src_labels[:, None])
        if self.train:
            self.pca.train(dataset.get_design_matrix())
        inputs = tensor.matrix()
        pca_transform = theano.function([inputs], self.pca(inputs))
        dst_data = pca_transform(dataset.get_design_matrix())
        return datasetformat_2_src(dst_data), src_labels

def autoencoder_encoder(in_out_structure, max_epoches = 1, save_path = None, load_path = None):
    return Encoder(
        get_layer_trainer_sgd_autoencoder,
        get_autoencoder(in_out_structure), max_epoches, save_path, load_path)

def denoising_autoencoder_encoder(in_out_structure, max_epoches = 1, save_path = None, load_path = None):
    return Encoder(
        get_layer_trainer_sgd_autoencoder,
        get_denoising_autoencoder(in_out_structure), max_epoches, save_path, load_path)

def rbm_encoder(in_out_structure, max_epoches = 1, save_path = None, load_path = None):
    return Encoder(
        get_layer_trainer_sgd_rbm,
        get_grbm(in_out_structure), max_epoches, save_path, load_path)

class another_rbm_encoder(Preprocessor):
    def __init__(self, rbm, max_epoches = 2, is_test = True) :
        self.rbm = rbm
        self.max_epoches = max_epoches
        self.is_test = is_test

    def run(self, src_data, src_labels):
        if self.is_test:
            src_data = rbm_perfrom(self.rbm, src_data)
        else:
            rbm_train(self.rbm, src_data, training_epochs=self.max_epoches)
            src_data = rbm_perfrom(self.rbm, src_data)
        return src_data, src_labels


