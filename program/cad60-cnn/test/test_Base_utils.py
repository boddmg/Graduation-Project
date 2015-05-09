__author__ = 'boddmg'

import unittest
import numpy
import Preprocessor.Base_utils

class IndexTestCase(unittest.TestCase):
    def runTest(self):
        src_data = numpy.array(
            [
                [
                    [0, 1, 2, 3, 4, 5, 6],
                    [10, 11, 12, 13, 14, 15, 16],
                ],
                [
                    [100, 101, 102, 103, 104, 105, 106],
                    [110, 111, 112, 113, 114, 115, 116]
                ]
            ]
        )
        src_labels = numpy.array([1, 2])
        dst_data, dst_labels = Preprocessor.Base_utils.Index([0, 3, 5]).run(src_data, src_labels)
        right_dst_data = numpy.array(
             [
                [
                    [0, 3, 5],
                    [10, 13, 15],
                ],
                [
                    [100, 103, 105],
                    [110, 113, 115]
                ]
            ],  dtype=numpy.float32
        )
        assert (dst_data == right_dst_data).all()




if __name__ == "__main__":
    unittest.main()

