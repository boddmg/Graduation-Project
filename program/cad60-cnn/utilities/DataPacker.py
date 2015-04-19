__author__ = 'boddmg'

from fuel import config
from fuel.datasets import IndexableDataset
from fuel.utils import do_not_pickle_attributes
from collections import OrderedDict

@do_not_pickle_attributes('indexables')
class PackerForFuel(IndexableDataset):
    provides_sources = ('features', 'targets')

    def __init__(self, data, label, **kwargs):
        self.newData = OrderedDict(zip(self.provides_sources , [data, label[:, None]] ))
        super(PackerForFuel, self).__init__(self.newData, **kwargs)

    def load(self):
        self.indexables = [data for source, data
                           in self.newData
                           if source in self.sources]

