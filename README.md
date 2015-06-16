<head> <meta charset="UTF-8"> </head>
#基于深度学习的人体运动类型识别  
##目录结构  
```  
|____根目录下各种报告
|____code
| |____.gitignore
| |____.gitmodules
| |____README.md
| |____document（写文档时的原稿和素材）
| |____program（放代码的地方）
| | |____model-experiment（存放实验代码的目录）
| | | |____handmake_preprocess.py（对自主录制的数据进行预处理的脚本，修改脚本中的目录然后运行即可）
| | | |____record.py（配合客户端录制骨架数据，输入的参数是当前录制任务的结果需要保存的文件的路径）
| | | |____cad60_cnn_in_pylearn2.py（**可以不看的实验代码——使用 pylearn2 做 CNN 的实验的程序，速度太慢，所以不予采用）
| | | |____cad60_cnn.py（**可以不看的实验代码——使用 blocks 第一个进行实验的程序）
| | | |____cad60_dataset4fuel.py（**可以不看的实验代码——把 CAD-60 数据集处理成 fuel 能使用的格式，fuel 是 blocks 专用的数据源管理包）
| | | |____cad60_dataset4pylearn2.py（**可以不看的实验代码——名字和上面的差不多）
| | | |____cad60_preprocess.py（**可以不看的实验代码——对数据进行无监督训练）
| | | |____cad60_skeleton.py（**可以不看的实验代码——读原始的 CAD-60 的文本数据进行处理的文件）
| | | |____cad60_train.hkl（**可以不看的实验代码——有监督训练的代码）
| | | |____handmake_dataset（手动录的数据集）
| | | | |____person2（按人分类的目录）
| | | |____demo-client（客户端发送骨架和接收、显示识别结果的程序）
| | | |____experiment（实验代码和结果的存放目录）
| | | | |____demo（通过网络进行通信识别的 DEMO 程序，需要把训练出来的 param-after-train.pkl 拷贝到该目录下，然后与性能 cad60_demo.py）
| | | | | |____cad60_demo.py（）
| | | | | |____cad60_record.py
| | | | | |____mock_client.py
| | | | | |____test-record-process.py
| | | | |____norm-3cnn-2mlp--regularization（目前为止效果最好的模型，已经改为使用自主录制的数据集，如果需要使用 CAD－60 的数据集，需要修改cad60_preprocess中读取的数据文件的路径）
| | | | | |______init__.py
| | | | | |____cad60_preprocess.py（无监督学习的部分，需要在有监督训练的前面运行）
| | | | | |____cad60_supervise_train.py（有监督学习，直接运行然后查看训练结果即可）
| | | | | |____test-record-process.py（读取有监督学习的输出的文本文件，处理成画图的代码放在粘贴板上，然后自动打开绘图的网站，这个时候只需要粘贴上去然后点“run”即可）
| | | |____Preprocessor（对数据进行预处理和无监督学习，写完只有自己用所以没有注释）
| | | | |______init__.py
| | | | |____Base_utils.py（基础的数据处理，读写文件、索引之类的）
| | | | |____dataset_utils.py（对特殊的数据集进行处理的部分）
| | | | |____encoder.py（无监督学习的模型，由于都可以理解为一种稀疏的编码器，所以命名为 encoder）
| | | | |____rbm.py（另一个 rbm 的实现）
| | | |____test
| | | | |______init__.py
| | | | |____test_Base_utils.py（数据处理的测试，不完整）
| | | |____utilities（不知道放哪的小工具就放这儿）
| | | | |______init__.py
| | | |____utils.py
| | |____tutorial（忽略）
| |____resource（忽略）
| |____cad60（cad-60的数据集里的骨架模型部分，全是文本，具体格式见[CAD-60](http://pr.cs.cornell.edu/humanactivities/data.php)）
| | |____data1（一个目录一个人）
| | | |____0512175502.txt（一个动作）
| | | |____activityLabel.txt（标签）
```  
##操作说明  
以下过程为录制数据集并进行训练的过程  
1.打开客户端应用程序目录下的```KinectExplorer-WPF.exe.config```配置客户端的连接 IP  

2.运行 ```./record.py handmake_dataset/person2/run.pkl``` 打开录制的程序并保存当前的动作到``` handmake_dataset/person2/run.pkl```，然后在客户端上运行```KinectExplorer-WPF.exe```软件开始录制，服务端软件键入```ctrl+c```来停止录制并保存。  

3.运行```./handmake_preprocess.py```进行录制数据的预处理，此时会输出每个文件和其对应的标签编号，需要根据这个输出的标签编号去修改客户端里的```Skeleton_sender.cs```文件中的```MOTION_TABLE```数组来改变显示的标签。  

4.运行模型目录下的```./cad60_preprocess.py```进行无监督训练  

5.运行```./cad60_supervise_train.py > result.txt```进行训练并把训练的信息保存到 result.txt 文件中。如果想要查看实时的训练信息可以运行 ```tail -f result.txt```来实时查看，打开浏览器登录 ```127.0.0.1:5006```来查看实时的识别率随训练过程的变化图像  

6.把训练好的 ```params-after-train.pkl``` 文件拷贝到 ```demo``` 目录下，运行 ```./demo.py``` 运行 demo 的识别程序  

7.运行客户端，开始识别。
