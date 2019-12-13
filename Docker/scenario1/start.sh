#!/bin/bash
service ngix start
systemctl start kestrel-helloapp.service
systemctl status kestrel-helloapp.service
exec $@
