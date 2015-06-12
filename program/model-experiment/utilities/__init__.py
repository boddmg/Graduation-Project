import pickle
from blocks.model import Model

def dump_params(tensor_var, file_name):
    pickle.dump(Model(tensor_var).get_param_values(), open(file_name,"w"))

def load_params(tensor_var, file_name):
    Model(tensor_var).set_param_values(pickle.load(open(file_name)))

