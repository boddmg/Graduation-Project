#!/usr/bin/env python
from fuel import config
from fuel.datasets import IndexableDataset
from fuel.utils import do_not_pickle_attributes

from collections import OrderedDict
import numpy
from cad60_skeleton import CAD60
from os import path
import sys

@do_not_pickle_attributes('indexables')
class CAD60Skeleton(IndexableDataset):
    provides_sources = ('features', 'targets')
    src_data = None
    src_labels = None

    def __init__(self, set_type = "train", batch_size = 10, flatten=False, **kwargs):
        self.flatten = flatten
        self.set_type = set_type
        self.batch_size = batch_size

        new_data = OrderedDict(zip(self.provides_sources, self._load_skeleton(self.batch_size)))

        super(CAD60Skeleton, self).__init__(new_data, **kwargs)

    def load(self):
        self.indexables = [data for source, data
                           in zip(self.provides_sources, self._load_skeleton(self.batch_size))
                           if source in self.sources]

    def _load_skeleton(self, batch_size):
        src_data, src_labels, temp = \
            CAD60(path.join(config.data_path, "cad60"),
                  batch_size,
                  self.set_type).get_data()

        indices = numpy.random.permutation(src_data.shape[0])
        src_data = src_data[indices]
        src_labels = src_labels[indices]
        print "shape:", src_data.shape, src_labels.shape

        return src_data, src_labels[:,None]


def main():
    print("start.")
    batch_size = 15
    if len(sys.argv)>1:
        batch_size = int(sys.argv[1])
    cad60_train = CAD60Skeleton("train", batch_size=batch_size)
    cad60_test = CAD60Skeleton("test", batch_size=batch_size)
    # next_batch = cad60.get_data_batch(10)
    # for i in next_batch:
    #     print(len(i[0][0]),i[1])

if __name__ == "__main__":
    main()
