#!/usr/bin/env python
import sys
import os
import re

import webbrowser
import clipboard

chart_template = '''
$(function () {
    $('#container').highcharts({
        title: {
            text: 'Experiment',
            x: -20 //center
        },
        subtitle: {
            text: '',
            x: -20
        },
        xAxis: {
            title: {
                text: 'Ecope'
            }
        },
        yAxis: {
            title: {
                text: 'Correct rate'
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }]
        },
        tooltip: {
            valueSuffix: '%'
        },
        legend: {
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'middle',
            borderWidth: 0
        },
        series: [{
            name: 'test',
            data: ##test##
        }, {
            name: 'train',
            data: ##train##
        }]
    });
});
'''
chart_website="http://www.hcharts.cn/test/index.php"

if __name__ == "__main__":
    if len(sys.argv) == 2:
        src_string = open(sys.argv[1]).read()
        test = map(lambda x:float(x)*100,re.findall("test_correct_rate: (.*)\n",src_string))
        train = [test[0]] + map(lambda x:float(x)*100,re.findall("train_correct_rate: (.*)\n",src_string))
        chart_text = chart_template.replace('##test##',str(test)).replace('##train##',str(train))
        clipboard.copy(chart_text)
        webbrowser.open(chart_website)
