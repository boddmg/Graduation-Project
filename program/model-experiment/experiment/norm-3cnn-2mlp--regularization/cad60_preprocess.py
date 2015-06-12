#!/usr/bin/env python
__author__ = 'boddmg'
import sys
import os

sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))

from cad60_skeleton import CAD60
from Preprocessor.Base_utils import *
from Preprocessor.dataset_utils import *
from Preprocessor.encoder import *
import numpy as np

class CAD60Loader(Preprocessor):
    def __init__(self, data_type = "train"):
        self.data_type = data_type
        pass

    def run(self, src_data = None, src_labels = None):
        src_data, src_labels, temp = CAD60(batch_size=1, data_type=self.data_type).get_data()
        return src_data, src_labels

CONFIDENCE_INDEX = [9, 13, 23, 27, 37, 41, 51, 55, 65, 69, 79, 83, 93, 97, 107, 111, 121, 125, 135, 139,
                  149, 153, 157, 161, 165, 169]
POSE_INDEX = list(set(range(170)) - set(CONFIDENCE_INDEX))
POSE_INDEX.sort()


def display(x):
    print(x)
    return x

def linear_map_generator(min_in, max_in, min_out, max_out):
    return lambda x: (float(x)-min_in)*(max_out-min_out)/(max_in-min_in) + min_out

def labels_remaper(data, labels):
    return data, display( np.asarray(map(linear_map_generator(0, 12, -2.5, 2.5), labels)))


def main():
    norm_param = {}
    for i in ["train", "test"]:
        src_data, src_labels = PreprocessorList([
            DataLoad("../../handmake_%s.hkl" % i),
            Normalization(norm_param, i == "test"),
            Monitor(),
            DataDump("%s_feature.hkl" % i)]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()

