#!/usr/bin/env python
__author__ = 'boddmg'
import sys
import os

sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))

from cad60_skeleton import CAD60
from Preprocessor.Base_utils import *
from Preprocessor.encoder import *


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


def main():
    for i in ["train", "test"]:
        src_data, src_labels = PreprocessorList([
            DataLoad("../../cad60_%s.hkl" % i),
            Index(POSE_INDEX),
            pca_encoder([144, 144], "PCA.pkl", "PCA.pkl" if i == "test" else None),
            Monitor(),
            DataDump("cad60_%s_feature.hkl" % i)]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()

