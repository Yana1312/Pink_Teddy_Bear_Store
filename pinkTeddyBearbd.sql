CREATE DATABASE  IF NOT EXISTS `storepinkteddybear_bd` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `storepinkteddybear_bd`;
-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: storepinkteddybear_bd
-- ------------------------------------------------------
-- Server version	8.0.43

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `inventory`
--

DROP TABLE IF EXISTS `inventory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory` (
  `IdInventory` int NOT NULL AUTO_INCREMENT,
  `ArticulToy` varchar(50) NOT NULL,
  `QuantityToys` int NOT NULL,
  PRIMARY KEY (`IdInventory`),
  KEY `ArticulToy` (`ArticulToy`),
  CONSTRAINT `inventory_ibfk_1` FOREIGN KEY (`ArticulToy`) REFERENCES `toy` (`ArticulToy`) ON DELETE CASCADE,
  CONSTRAINT `inventory_chk_1` CHECK ((`QuantityToys` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inventory`
--

LOCK TABLES `inventory` WRITE;
/*!40000 ALTER TABLE `inventory` DISABLE KEYS */;
INSERT INTO `inventory` VALUES (1,'PTB001',13),(2,'PTB002',12),(3,'PTB003',12),(4,'PTB004',15),(5,'PTB005',5),(7,'PTB006',9),(9,'PTB007',5);
/*!40000 ALTER TABLE `inventory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order`
--

DROP TABLE IF EXISTS `order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order` (
  `IdOrder` int NOT NULL AUTO_INCREMENT,
  `IdCustomer` int NOT NULL,
  `DateOrder` datetime DEFAULT CURRENT_TIMESTAMP,
  `StatusOrder` varchar(50) DEFAULT 'ожидает подтверждения',
  `AdressOrder` varchar(500) NOT NULL,
  `TotalAmount` decimal(10,2) DEFAULT '0.00',
  PRIMARY KEY (`IdOrder`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order`
--

LOCK TABLES `order` WRITE;
/*!40000 ALTER TABLE `order` DISABLE KEYS */;
INSERT INTO `order` VALUES (2,2,'2024-01-16 14:20:00','отгружен','г. Санкт-Петербург, Невский пр-т, д. 100, кв. 32',1599.50),(3,3,'2024-01-17 16:45:00','в обработке','г. Казань, ул. Баумана, д. 45, кв. 7',1899.00),(4,1,'2025-10-23 15:46:32','в обработке','г. Москва, ул. Тверская, д. 25, кв. 14',4350.00),(5,4,'2024-01-19 11:30:00','доставлен','г. Екатеринбург, ул. Ленина, д. 78, кв. 21',2999.99),(7,22,'2025-11-06 11:45:43','в обработке','sasas',12896.99),(10,22,'2025-11-07 08:35:21','в обработке','г. Москва, ул. Тверская, д. 25, кв. 14',7997.49),(14,22,'2025-11-18 21:08:17','ожидает подтверждения','',6248.49),(15,26,'2025-11-18 23:00:22','ожидает подтверждения','',24516.91),(16,28,'2025-11-20 13:47:44','ожидает подтверждения','',3245.99),(18,30,'2025-11-20 15:46:31','доставлен','г. Москва, ул. Тверская, д. 25, кв. 14',4798.49),(20,30,'2025-11-20 19:37:25','в обработке','г. Москва, ул. Тверская, д. 25, кв. 14',41466.99),(23,30,'2025-11-20 20:39:40','отгружен','',1299.99),(24,31,'2025-11-20 23:36:23','в обработке','г. Казань, ул. Баумана, д. 45, кв. 7',12345.00),(25,33,'2025-11-21 08:36:02','отгружен','г. Москва, ул. Тверская, д. 25, кв. 14',38989.89),(33,33,'2025-11-24 23:19:47','доставлен','г. Москва, ул. Тверская, д. 25, кв. 14',1450.00),(34,33,'2025-11-25 00:36:27','в обработке','г. Москва, ул. Тверская, д. 25, кв. 14',12345.00),(35,33,'2025-11-25 02:03:09','ожидает подтверждения','',0.00),(36,1,'2025-11-25 04:00:01','ожидает подтверждения','',22294.46);
/*!40000 ALTER TABLE `order` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orderitem`
--

DROP TABLE IF EXISTS `orderitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orderitem` (
  `IdOrderItem` int NOT NULL AUTO_INCREMENT,
  `IdOrder` int NOT NULL,
  `ArticulToy` varchar(50) NOT NULL,
  `Quantity` int NOT NULL,
  `UnitPrice` decimal(10,2) NOT NULL,
  PRIMARY KEY (`IdOrderItem`),
  KEY `IdOrder` (`IdOrder`),
  KEY `ArticulToy` (`ArticulToy`),
  CONSTRAINT `orderitem_ibfk_1` FOREIGN KEY (`IdOrder`) REFERENCES `order` (`IdOrder`) ON DELETE CASCADE,
  CONSTRAINT `orderitem_ibfk_2` FOREIGN KEY (`ArticulToy`) REFERENCES `toy` (`ArticulToy`) ON DELETE RESTRICT,
  CONSTRAINT `orderitem_chk_1` CHECK ((`Quantity` > 0)),
  CONSTRAINT `orderitem_chk_2` CHECK ((`UnitPrice` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=119 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orderitem`
--

LOCK TABLES `orderitem` WRITE;
/*!40000 ALTER TABLE `orderitem` DISABLE KEYS */;
INSERT INTO `orderitem` VALUES (3,2,'PTB002',1,1599.50),(4,3,'PTB003',1,1899.00),(5,4,'PTB004',3,1450.00),(6,5,'PTB005',1,2999.99),(9,7,'PTB002',2,1599.50),(10,7,'PTB003',2,1899.00),(11,7,'PTB004',2,1450.00),(12,7,'PTB005',1,2999.99),(21,10,'PTB001',1,1299.99),(22,10,'PTB002',3,1599.50),(23,10,'PTB003',1,1899.00),(35,14,'PTB001',1,1299.99),(36,14,'PTB002',1,1599.50),(37,14,'PTB003',1,1899.00),(38,14,'PTB004',1,1450.00),(40,15,'PTB003',5,1899.00),(41,15,'PTB001',9,1299.99),(42,15,'PTB002',2,1599.50),(43,15,'PTB006',1,123.00),(46,16,'PTB005',1,2999.99),(47,16,'PTB006',2,123.00),(49,18,'PTB001',1,1299.99),(50,18,'PTB002',1,1599.50),(51,18,'PTB003',1,1899.00),(60,20,'PTB002',2,1599.50),(61,20,'PTB003',8,1899.00),(62,20,'PTB004',14,1450.00),(63,20,'PTB006',12,123.00),(64,20,'PTB001',1,1299.99),(71,23,'PTB001',1,1299.99),(72,24,'PTB006',1,12345.00),(73,25,'PTB001',11,1299.99),(78,25,'PTB006',2,12345.00),(108,33,'PTB004',1,1450.00),(112,34,'PTB006',1,12345.00),(113,35,'PTB007',2,10000.00),(114,36,'PTB001',3,1299.99),(115,36,'PTB002',1,1599.50),(116,36,'PTB004',1,1450.00),(117,36,'PTB005',1,2999.99),(118,36,'PTB006',1,12345.00);
/*!40000 ALTER TABLE `orderitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `review`
--

DROP TABLE IF EXISTS `review`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `review` (
  `IdReview` int NOT NULL AUTO_INCREMENT,
  `ArticulToy` varchar(50) NOT NULL,
  `IdCustomer` int NOT NULL,
  `RatingReview` tinyint NOT NULL,
  `CommentReview` text,
  `DateReview` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdReview`),
  KEY `ArticulToy` (`ArticulToy`),
  KEY `IdCustomer` (`IdCustomer`),
  CONSTRAINT `review_ibfk_1` FOREIGN KEY (`ArticulToy`) REFERENCES `toy` (`ArticulToy`) ON DELETE CASCADE,
  CONSTRAINT `review_ibfk_2` FOREIGN KEY (`IdCustomer`) REFERENCES `useransadmin` (`IdCustomer`) ON DELETE CASCADE,
  CONSTRAINT `review_chk_1` CHECK (((`RatingReview` >= 1) and (`RatingReview` <= 5)))
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `review`
--

LOCK TABLES `review` WRITE;
/*!40000 ALTER TABLE `review` DISABLE KEYS */;
INSERT INTO `review` VALUES (1,'PTB001',1,5,'Очень мягкий и качественный мишка! Дочка в восторге!','2024-01-20 12:00:00'),(3,'PTB001',3,5,'Прекрасное качество, идеальный подарок!','2024-01-22 10:15:00'),(4,'PTB005',26,5,'Роскошный мишка! Стоит своих денег!','2024-01-23 16:45:00'),(5,'PTB003',26,4,'Мягкий и уютный, но пижама могла бы быть качественнее','2024-01-24 08:20:00'),(6,'PTB005',26,4,'Очень мягкий, но слишком розовый','2025-10-22 09:13:53'),(7,'PTB002',26,4,'Очень мягкий мишка, но слишком розовый','2025-10-22 09:19:09'),(8,'PTB001',26,3,'бе','2025-10-23 15:52:05'),(9,'PTB001',27,2,'qweqweqweqwe','2025-11-19 00:14:14'),(10,'PTB001',28,3,'Игрушка простая','2025-11-20 15:03:26'),(11,'PTB002',28,5,'Ваууу, я в восторге!','2025-11-20 13:31:16'),(12,'PTB001',30,5,'weerdgfhjk','2025-11-20 21:29:15'),(13,'PTB001',31,4,'qwertyuio','2025-11-20 23:32:45'),(14,'PTB002',31,5,'Очень хорошее качество','2025-11-20 23:35:23'),(15,'PTB001',33,5,'Игрушка очень классная','2025-11-21 08:34:27'),(16,'PTB002',33,4,'Игрушка не очень','2025-11-21 08:34:40'),(17,'PTB003',33,4,'Пойдет','2025-11-24 21:29:39');
/*!40000 ALTER TABLE `review` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `toy`
--

DROP TABLE IF EXISTS `toy`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `toy` (
  `ArticulToy` varchar(50) NOT NULL,
  `Title` varchar(200) NOT NULL,
  `Descriptionn` text,
  `Price` decimal(10,2) NOT NULL,
  `Height` varchar(10) DEFAULT NULL,
  `Weight` varchar(10) DEFAULT NULL,
  `QuantityInStock` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`ArticulToy`),
  CONSTRAINT `toy_chk_1` CHECK ((`Price` >= 0)),
  CONSTRAINT `toy_chk_2` CHECK ((`QuantityInStock` >= 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `toy`
--

LOCK TABLES `toy` WRITE;
/*!40000 ALTER TABLE `toy` DISABLE KEYS */;
INSERT INTO `toy` VALUES ('PTB001','Нежность','Мягкий плюшевый мишка розового цвета с бантиком',1299.99,'25 см','300 г',13),('PTB002','Валентинка','Розовый мишка с сердечком, идеальный подарок на День влюбленных',1599.50,'30 см','400 г',12),('PTB003','Сладкий сон','Нежный розовый мишка в пижаме, для крепкого сна',1899.00,'35 см','500 г',0),('PTB004','Весенний','Светло-розовый мишка с цветочным аксессуаром',1450.00,'28 см','350 г',14),('PTB005','Премиум класс','Большой роскошный розовый мишка из высококачественного плюша',2999.99,'50 см','800 г',4),('PTB006','Тестовая игрушка','Тестовый пух',12345.00,'12','123',5),('PTB007','Туфли','Ого, это что туфли, а не мишка? Нет, это просто тест)',10000.00,'170','53',5);
/*!40000 ALTER TABLE `toy` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `useransadmin`
--

DROP TABLE IF EXISTS `useransadmin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `useransadmin` (
  `IdCustomer` int NOT NULL AUTO_INCREMENT,
  `EmailUsers` varchar(255) NOT NULL,
  `NameUsers` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `StatusUsersProfile` varchar(45) NOT NULL,
  `RoleUsers` varchar(45) NOT NULL,
  PRIMARY KEY (`IdCustomer`),
  UNIQUE KEY `EmailCustomer` (`EmailUsers`)
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `useransadmin`
--

LOCK TABLES `useransadmin` WRITE;
/*!40000 ALTER TABLE `useransadmin` DISABLE KEYS */;
INSERT INTO `useransadmin` VALUES (1,'anapetrova293@gmail.com','Анна Иванова','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','активный','пользователь'),(2,'petr.sidorov@gmail.com','Петр Сидоров','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','неактивный','пользователь'),(3,'maria.petrova@yandex.ru','Мария Петрова','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','неактивный','пользователь'),(4,'serg.kuznetsov@mail.ru','Сергей Кузнецов','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','неактивный','пользователь'),(5,'olga.vorobeva@gmail.com','Ольга Воробьева','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','неактивный','пользователь'),(6,'askld@gmail.com','dsdfd','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','неактивный','пользователь'),(7,'fklldfkj@mail.ru','Яна','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','неактивный','пользователь'),(8,'qwe@gmail.com','Яночка','$2a$11$Z4NxA.rL.PxK0T.vknaoe.6ci0YZqe0EJ2fUTAqzOu2N8iiOdXkc6','неактивный','пользователь'),(10,'qweasd@gmail.com','string','$2a$11$KgHmThZb4O4xciow5o4e5OfAKioFM1RXYpy3o0A50pCv.l8FasmaS','неактивный','админ'),(11,'anapet@gmail.com','Yana','$2a$11$D89cd9WDjGCLLQzLFWTzKO7SPLdjYQ/dYqgbuf4/nbD3/wX6kzFrq','неактивный','админ'),(13,'strem@gmail.com','string','$2a$11$2HwKvAfRhEpy08KZoJm8JuYnTQMCYwf8dhSgPqAxMeQIYxpqtxZ6y','активный','пользователь'),(14,'stringjj@gmail.com','string','$2a$11$so9Cq1P.XJ4Qv7epghR9w.apzePPhWHuBHxrtNc8aCy37G9CnLbLW','активный','пользователь'),(15,'asd@gmail.com','Яна','$2a$11$Yhi6Pi2oWf.2wZnZ4oD.FOy9i4yJvePdGnSTpmUEu7.PKr3G9eplm','активный','пользователь'),(16,'qwaszx@gmail.com','ЯночкаЛучшая','$2a$11$JranovSQbAkWyrP2jZWimuwfwdJnAszFGoGzhjeahbwDdLnIybU9u','активный','пользователь'),(17,'ascxc@gmail.com','Utka','$2a$11$oC/o9YWvD.iPbjPHDg7JFu5dQ6RvLYLS0aj8aG7toP53M0vlajug.','активный','пользователь'),(18,'anapet12@gmail.com','qwe','$2a$11$NVKm4mS4B2ixPb9mqd6gru/i3OvNYtmlZcc3cTYVCV1PXqy./ayVa','активный','пользователь'),(19,'qween@gmail.com','Яна','$2a$11$VZyUT6xXNKd.cRNGrHowquAvwZ6xB.ka6Z0FkVoZqni1Rnt6sfPpa','активный','админ'),(20,'aqwsd@gmail.com','aasdwdsc','$2a$11$TmI.b6BeyZQqJdCcUugVvu3DRLSP8Xjhs6nGDMdREWU2OJIEtExqW','активный','пользователь'),(21,'qweasd123@gmail.com','Яночка','$2a$11$DWiabZC.oCxwdN/uJk0xIu6fXAqy8TfO.9dL5MQ6eOQyqUDs48KC.','активный','пользователь'),(22,'qween1@gmail.com','Янв','$2a$11$BP1LpoksDtclIE1KgSz6qOBLUBEwZ4nrBTiW0J.vI/1BmnqdE4/.C','неактивный','пользователь'),(23,'qweasd12@gmail.com','retgrfe','$2a$11$DMV7qlyvPTZWGtSomI38XuO52OLYWOtt/ZIl6hCCzS3yvsg8vMT1e','активный','пользователь'),(24,'qwe13@gmail.com','Яночка','$2a$11$lKmU8offSbvQ45EHcEM5CuZn7PTR4fclBGADl3Gxbr7BE4XvEau0S','активный','пользователь'),(25,'qwe12@gmail.com','qweasd','$2a$11$jGkeB7En6O44gXkCmXW6O.Q/dE35/jx5TOIePBBIq6l6fXBApPvkC','активный','пользователь'),(26,'qween2@gmail.com','Яночка','$2a$11$k6kISTrgYtfJ/HtMmqaf7OLGBiIv7TxEscS474ubQp8kNW1TrzhQe','активный','пользователь'),(27,'qwas@gmail.com','Янчик-Цветок','$2a$11$a9v1JFl6iyBYeyFg05uw1ufBTRG/1Hr9mCBtkEWuW6i1FzIhC/hXq','активный','пользователь'),(28,'qween3@gmail.com','Яночка','$2a$11$x5NCl2ik6gOiB75Ilycmku9n7pI5SHfERu1V6RpxHzT.kM0/GzhrW','активный','пользователь'),(29,'qweasd13@gmail.com','Яна4','$2a$11$l45EWM2qOjDzmTd2abiRYuxDEdIz4rrlCdAHaOe3nfT9jQHLHjGw6','активный','пользователь'),(30,'qweasd14@gmail.com','qweasd','$2a$11$YlgdyU1NW93cX8dejTMOEOXuyKOLS5Jiy233iPaX7LGNK76XFxHD.','активный','пользователь'),(31,'sad123@gmail.com','Яночка','$2a$11$tULAb06MuukY/VjQrNj/oOzs9OHzf2770Vio9sy56mx9jXjFnuTZ6','активный','пользователь'),(32,'y38843641@gmail.com','Янаааааааааааааа','$2a$11$bzWvfXQ.KeFkGpwDUItD7.s1Ij.DCZWLSm1t.G8gF4HvVZO8WYtiq','активный','пользователь'),(33,'qweasdqwee@gmail.com','qweasdqwee','$2a$11$xNx9pkXybQyOAI4JXxqqj.QjY9Dh/RqJ0DEsSv8trnoHI1jHDrngu','активный','пользователь');
/*!40000 ALTER TABLE `useransadmin` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-11-25  4:09:09
