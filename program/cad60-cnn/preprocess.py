#!/usr/bin/env python
__author__ = 'boddmg'


from cad60_skeleton import CAD60
from fuel import config
import numpy

class Preprocessor(object):
    def __init__(self):
        pass

    def run(self, src_data, src_labels):
        pass

class PreprocessorList(Preprocessor):
    def __init__(self, list):
        self.list = list
        pass

    def run(self, src_data, src_labels):
        for i in self.list:
            src_data, src_labels = i.run(src_data, src_labels)
        return src_data, src_labels


class Shuffle(Preprocessor):
    def __init__(self):
        pass

    def run(self, src_data, src_labels):
        indices = numpy.random.permutation(src_data.shape[0])
        src_data = src_data[indices]
        src_labels = src_labels[indices]
        return src_data, src_labels

class GetBatch(Preprocessor):
    def __init__(self, batch_size = 1, stride = 1):
        self.batch_size = batch_size
        self.stride = stride

    def run(self, src_data, src_labels):
        return src_data, src_labels


def main():
    src_data, src_labels, temp = CAD60(batch_size=1, data_type="train").get_data()
    src_data, src_labels = PreprocessorList([Shuffle()]).run(src_data,src_labels)
    print src_data, src_labels

if __name__ == '__main__':
    main()
