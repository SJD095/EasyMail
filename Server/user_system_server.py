import textwrap

import tornado.httpserver
import tornado.ioloop
import tornado.options
import tornado.web

from tornado.options import define, options
define("port", default=7000, help="run on the given port", type=int)

user_password = {}

class loginHandler(tornado.web.RequestHandler):
    def post(self):
	result = ""
        text = self.request.body
	text = str(text)
	username, password = text.split('\t')
	if user_password.has_key(username):
		if user_password[username] == password:
			result = "Login success"
		else:
			result = "Error password"
	else:
		result = "Error user"
	self.write(result)

class registerHandler(tornado.web.RequestHandler):
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

class changeHandler(tornado.web.RequestHandler):
    def post(self):
	result = ""
	text = self.request.body
	text = str(text)
	username, oldpassword, newpassword = text.split('\t')
	if user_password.has_key(username):
		if user_password[username] == oldpassword:
			user_password[username] = newpassword
			result = "change success"
		else:
			result = "oldpassword wrong"
	else:
		result = "No account"
	self.write(result)

if __name__ == "__main__":
    tornado.options.parse_command_line()
    app = tornado.web.Application(
        handlers=[
            (r"/login", loginHandler),
            (r"/register",registerHandler),
	    (r"/change", changeHandler)
        ]
    )
    http_server = tornado.httpserver.HTTPServer(app)
    http_server.listen(options.port)
    tornado.ioloop.IOLoop.instance().start()
