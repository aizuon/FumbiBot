-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               10.3.15-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Version:             10.1.0.5464
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping structure for table discord.inventory
CREATE TABLE IF NOT EXISTS `inventory` (
  `Uid` bigint(20) unsigned NOT NULL,
  `Alice` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Alice2` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Glitch` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `IronEyes` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Kitty` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Lilith` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Ophelia` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Pug` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Hitler` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Hitler2` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `GlitchAnim` tinyint(3) unsigned NOT NULL DEFAULT 0,
  UNIQUE KEY `Uid` (`Uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.
-- Dumping structure for table discord.users
CREATE TABLE IF NOT EXISTS `users` (
  `Uid` bigint(20) unsigned NOT NULL,
  `Name` text DEFAULT NULL,
  `Level` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `Exp` int(10) unsigned NOT NULL DEFAULT 0,
  `Pen` bigint(20) unsigned NOT NULL DEFAULT 0,
  `ProfileTheme` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `LastDaily` text DEFAULT NULL,
  `DailyExp` text DEFAULT NULL,
  UNIQUE KEY `Uid` (`Uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
