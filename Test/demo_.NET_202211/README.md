### 项目介绍
此项目是上上签为方便C#开发者使用赋能版API而编写的Demo，在Demo中详细的展示了如何获取Token、计算签名、组装参数、调用上上签赋能版API以及处理返回结果。

此Demo包含了两个常见的签署流程：
1. 合同签署主流程：  
    创建并发送合同 -> 签署合同（自动签）-> 查询合同详情 -> 下载合同 -> 下载附页
2. 模板预览与发送：  
    查询模板详情 -> 生成预览文件 -> 模板发送合同

>Note:
    本项目使用了C#的开源组件cURL，在体验Demo之前请确保本机环境开启了此扩展。

### 项目结构
- Contract.cs Template.cs : 接口的参数及调用封装
    - 对应开放平台文档**合同发送和签署、签约状态列表、撤销/拒签、合同下载**
-  常用的工具
    - BestSignHttpClient.cs  HttpSender.cs: 封装Token获取、签名计算及响应处理
- data : 测试文件  
- : 
  - ContractApiTest.java 和 ContractTemplateTest.java :   签署流程Demo
  - BestSignTest.java 系统配置参数
  
### demo完成功能
1. 浏览合同
2. 发送合同 ：使用合同模发送合同(新接口)
3. 发送合同 ： 使用合同模发送合同(老接口)
4. 发送合同 ： 不使用合同模板
5. 多关键字定位
6. 下载合同附页
7. 下载单份合同
8. 下载多份合同


### 如何使用
1. 在Demo.ini文件中填写相应的参数 serverHost， clientId ，clientSecret， privateKey
2. 执行Demo.cs 和 ContractTest.cs 相应功能

