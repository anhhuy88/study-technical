# install ssh-server
 - sudo apt update
 - sudo apt upgrade
 - sudo apt install openssh-server
 - sudo systemctl enable ssh
 - sudo systemctl start ssh
 - sudo ufw allow ssh
 - sudo ufw enable
 - sudo ufw status

# commands
- Permission: sudo -i

## Firewall
- sudo ufw allow [port]
- sudo ufw deny [port]
- sudo ufw disable
- sudo ufw enable
- sudo ufw reset
- Liệt kê các cổng: lsoft -i -P -n
- xóa cổng: kill [PID]
- Danh sách các cổng: netstat -tln
- Renew ip: sudo dhclient -v

https://www.digitalocean.com/community/tutorials/how-to-set-up-a-firewall-with-ufw-on-ubuntu#step-3-allowing-ssh-connections
https://www.inmotionhosting.com/support/security/open-a-port-in-ufw/

sudo netstat -lptu


## Raspberry mở cổng sử dụng như sau: firewalld
- sudo apt-get install firewalld
- sudo firewall-cmd --add-port=80/tcp --permanent
- sudo firewall-cmd --add-port=443/tcp --permanent
- sudo firewall-cmd --reload
- sudo firewall-cmd --list-all
- Reference: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-8.0


## Postgresql
- Install:
+ sudo apt update
+ sudo apt install postgresql postgresql-contrib
+ psql --version
+ Check the database status: sudo service postgresql status
+ sudo service postgresql start
+ sudo service postgresql stop
+ The default admin user: postgres
+ Change password: sudo passwd postgres
+ Reference: https://learn.microsoft.com/en-us/windows/wsl/tutorials/wsl-database#install-postgresql
- Kiểm tra phiên bản: psql --version
- 


## Convert MSSQL to Postgresql
### 1. Install wsl
- Powershel: wsl --install
+ Hoặc chỉ định phiên bản Ubuntu: wsl --install -d Ubuntu-22.04
- Cài đặt WSL 2 mặc định: wsl --set-default-version 2
- Cập nhật hệ thống: sudo apt update && sudo apt upgrade

### 2. Install Postgresql
- sudo apt update
- sudo apt install postgresql postgresql-contrib
- Kiểm tra phiên bản: psql --version
- Check the database status: sudo service postgresql status
- sudo service postgresql start
- sudo service postgresql stop
- The default admin user: postgres
	+ Change password: sudo passwd postgres
-  Backup & restore DB
+ createdb -U postgres -O sa demoDb4
+ pg_dump -h localhost -U postgres -F c -d demoDb2 > /home/huypv/demoDb2.dump
+ pg_restore -h localhost -U postgres -d demoDb4 /home/huypv/demoDb2.dump --clean
+ dropdb -h localhost -U postgres demoDb4
+ Cách xóa DB khác: 
	* psql -U postgres -d postgres
	* SELECT pg_terminate_backend(pg_stat_activity.pid)
		FROM pg_stat_activity
		WHERE pg_stat_activity.datname = 'demoDb1'
		AND pid <> pg_backend_pid();
	* DROP DATABASE "demoDb1";
	* Hoặc: DROP DATABASE "demoDb2" WITH (FORCE);

- Để remote từ DBeaver bên ngoài cần thiết lập peer -> md5
+ Tệp: sudo nano /etc/postgresql/16/main/pg_hba.conf

- Quy trình đổi mật khẩu cho tài khoản gốc: postgres
+ Go to (Đổi peer thành trust > CTR + O > CTR + X): sudo nano /etc/postgresql/16/main/pg_hba.conf
+ sudo systemctl restart postgresql
+ sudo -u postgres psql
+ ALTER USER postgres WITH PASSWORD 'mat_khau_moi'; \q;
+ Go to (Đổi trust thành md5 > CTR + O > CTR + X): sudo nano /etc/postgresql/16/main/pg_hba.conf
+ sudo systemctl restart postgresql


- Reference: https://learn.microsoft.com/en-us/windows/wsl/tutorials/wsl-database#install-postgresql

### 3. Download DBeaver
- Reference: https://dbeaver.io/download/

### 4. Configuration MSSQL
- Go to "Sql Server Configuration Manager" > SQL Server Network Configuration > TCP/IP > Enable
- Double click "TCP/IP" > Tab "IP Addresses" > IPALL > TCP Port = 1433
- Nếu cần thiết thì Open port: Windows Defender Firewall with Advanced Security > Inbound rules > New rule... > Input port: 1433 

### 5. Move MSSQL data to Postgresql data
Có 2 phương pháp: sử dụng DBeaver hoặc sử dụng lệnh bcp xuất CSV sau đó sử dụng psql để import vào DB.
Sau đây là sử dụng phương pháp sử dụng DBeaver
- 1. Mở DBeaver, Kết nối đến MSSQL và Postgresql
- 2. Tại MSSQL chọn tên CSDL. Chọn những bảng muốn sao chép.
- 3. Right-click and select "Export Data.": https://prnt.sc/X0T-ywHsz344
	+ Export target > database > Next
	+ Choose database target > Next
	+ Next ... Procced > Done.


# Triển khai .net core app

# Cài đặt Nginx

# Cài đặt SSL cho Website

# Share folder between windows host and ubuntu guest machine
1. Select ubuntu VM on Virtualbox
3. Click **Setting** button
4. Select **Share Folders** / **Choose folder on windows host** / Okay
5. Start ubuntu virtualbox
6. Install packages
- sudo apt-get update
- sudo apt-get install virtualbox-guest-dkms
- sudo apt-get install virtualbox-guest-utils
- reboot
7. sudo mount -t vboxsf [name folder share] [path on ubuntu machine]
8. set auto amount: echo "sudo mount -t vboxsf [name folder share] [path on ubuntu machine]" >> /home/myusername/.profile
- References: [Share folder between windows host and ubuntu guest machine](https://stackoverflow.com/questions/54336626/how-to-create-virtualbox-shared-folder-between-windows-host-and-ubuntu18-04-gues)

# Tài liệu tham khảo
- https://viblo.asia/p/tim-hieu-va-huong-dan-setup-web-server-nginx-OREGwBwlvlN
- 