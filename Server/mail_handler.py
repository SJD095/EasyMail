#!/usr/bin/python
# -*- coding: UTF-8 -*-

import textwrap

import tornado.httpserver
import tornado.ioloop
import tornado.options
import tornado.web

#设置接收端口为7001
from tornado.options import define, options
define("port", default=7001, help="run on the given port", type=int)

#设置邮件收件人为键，邮件内容为值的字典
receiver_mailcontent = {}

#处理发件请求的类
class sendHandler(tornado.web.RequestHandler):
    def post(self):

        #获取报文
        result = ""
        text = self.request.body
        text = str(text)

        #从报文中取出收件名和邮件内容
        receiver, mailcontent = text.split('\t')

        #设置词典中对应收件人的内容
        receiver_mailcontent[receiver] = mailcontent

        #写入成功报文
        result = "success"
        self.write(result)

class checkHandler(tornado.web.RequestHandler):
    def post(self):

        #获取报文
        result = ""
        text = self.request.body
        text = str(text)

        #确定字典中是否有制定的用户名所对应的邮件，如果没有则返回No
        if receiver_mailcontent.has_key(text):
            if receiver_mailcontent[text] != "":
                result = receiver_mailcontent[text]

                #获取返回邮件后将字典中收件人对应的邮件置空
                receiver_mailcontent[text] = ""
            else:
                result = "No"
        else:
            result = "No"

        self.write(result)

if __name__ == "__main__":
    tornado.options.parse_command_line()
    app = tornado.web.Application(
        handlers=[
            (r"/send", sendHandler),
            (r"/check",checkHandler)
        ]
    )
    http_server = tornado.httpserver.HTTPServer(app)
    http_server.listen(options.port)
    tornado.ioloop.IOLoop.instance().start()
