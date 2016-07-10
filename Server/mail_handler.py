import textwrap

import tornado.httpserver
import tornado.ioloop
import tornado.options
import tornado.web

from tornado.options import define, options
define("port", default=7001, help="run on the given port", type=int)

class checkHandler(tornado.web.RequestHandler):
    def post(self):
	result = ""
        text = self.request.body
	text = str(text)
        username, password = text.split('\t')
	if user_password.has_key(username):
		result = "User exist"
        else:
		user_password[username] = password
		result = "register success"
	self.write(result)

if __name__ == "__main__":
    tornado.options.parse_command_line()
    app = tornado.web.Application(
        handlers=[
            (r"/check", checkHandler)
        ]
    )
    http_server = tornado.httpserver.HTTPServer(app)
    http_server.listen(options.port)
    tornado.ioloop.IOLoop.instance().start()
