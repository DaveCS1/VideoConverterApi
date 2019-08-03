# Video Converter API

https://videoconverter.azurewebsites.net/swagger

Assumptions & Explanations:


1.	In this project I assumed max content size will be 2gb as max allowed in IIS 7. In order to deal with larger file I would use chunk / stream upload

2.	In this project, I have not considered async upload & proccessing.
To deal with async proccesing & upload I would use async api action and architecture that uses queues.

3.	I did not handled lazyLoad/paging data select etc – for real production product it should definitely be handled in this kind of project.

4.	For DB stuff to keep it simple - I chose to use one of my own remote mySql DB with sqlCommand queries, I'm aware of the option to use either EF, Nhibernate or writing Stored Procedures in the DB

5.	No multiple files upload, only one file on each request.

6.	I didn't use any specific dependancy injection pattern on the backend because I find it unnecessary for this small project.

7.	I chose to implement the task in .net CORE Api which I'm not so familiar with.

8.	About HLS files – I'm more familiar with mpeg DASH, have some experience with creating multiple scaled files & smil file for WOWZA streaming server.

9.	To Store images & videos I would choose some NAS storage, In this project I created a virtual directory in IIS for the WebAPI host that points the videos folder which store all files 
 (e.g to access some thumbnail http://localhost/VideoConverterApi/videos/sample-Elysium.2013.2160p/sample-Elysium.2013.2160p_thumbnail_3.jpg)
