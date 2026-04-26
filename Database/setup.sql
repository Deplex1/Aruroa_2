-- =============================================
-- Aurora Music Platform - Database Setup Script
-- =============================================
-- This script creates all necessary tables for the Aurora Music platform
-- Run this script after creating the 'auroradb' database

USE auroradb;

-- =============================================
-- Table: users
-- Stores user account information
-- =============================================
CREATE TABLE IF NOT EXISTS users (
    userid INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,  -- SHA256 hashed
    email VARCHAR(100) NOT NULL UNIQUE,
    profilepicture LONGBLOB NULL,
    IsAdmin INT DEFAULT 0,  -- 0 = regular user, 1 = admin
    ResetCode VARCHAR(100) NULL,
    INDEX idx_username (username),
    INDEX idx_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Table: genres
-- Stores music genres
-- =============================================
CREATE TABLE IF NOT EXISTS genres (
    genreid INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    INDEX idx_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Table: songs
-- Stores uploaded songs with audio data
-- =============================================
CREATE TABLE IF NOT EXISTS songs (
    songid INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    duration INT NOT NULL,  -- Duration in seconds
    audioData LONGBLOB NOT NULL,  -- MP3/WAV/AAC file
    userid INT NOT NULL,
    uploaded DATETIME DEFAULT CURRENT_TIMESTAMP,
    plays INT DEFAULT 0,
    FOREIGN KEY (userid) REFERENCES users(userid) ON DELETE CASCADE,
    INDEX idx_title (title),
    INDEX idx_userid (userid),
    INDEX idx_plays (plays),
    INDEX idx_uploaded (uploaded)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Table: song_genres (Junction Table)
-- Links songs to multiple genres (many-to-many)
-- =============================================
CREATE TABLE IF NOT EXISTS song_genres (
    songid INT NOT NULL,
    genreid INT NOT NULL,
    PRIMARY KEY (songid, genreid),
    FOREIGN KEY (songid) REFERENCES songs(songid) ON DELETE CASCADE,
    FOREIGN KEY (genreid) REFERENCES genres(genreid) ON DELETE CASCADE,
    INDEX idx_songid (songid),
    INDEX idx_genreid (genreid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Table: playlists
-- Stores user-created playlists
-- =============================================
CREATE TABLE IF NOT EXISTS playlists (
    playlistid INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    userid INT NOT NULL,
    ispublic BOOLEAN DEFAULT FALSE,
    created DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (userid) REFERENCES users(userid) ON DELETE CASCADE,
    INDEX idx_userid (userid),
    INDEX idx_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Table: playlistsongs (Junction Table)
-- Links songs to playlists with position ordering
-- =============================================
CREATE TABLE IF NOT EXISTS playlistsongs (
    playlistid INT NOT NULL,
    songid INT NOT NULL,
    position INT NOT NULL,
    dateadded DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (playlistid, songid),
    FOREIGN KEY (playlistid) REFERENCES playlists(playlistid) ON DELETE CASCADE,
    FOREIGN KEY (songid) REFERENCES songs(songid) ON DELETE CASCADE,
    INDEX idx_playlistid (playlistid),
    INDEX idx_position (position)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Table: ratings
-- Stores user ratings for songs (1-5 stars)
-- =============================================
CREATE TABLE IF NOT EXISTS ratings (
    ratingid INT AUTO_INCREMENT PRIMARY KEY,
    userid INT NOT NULL,
    songid INT NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    daterated DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_user_song (userid, songid),
    FOREIGN KEY (userid) REFERENCES users(userid) ON DELETE CASCADE,
    FOREIGN KEY (songid) REFERENCES songs(songid) ON DELETE CASCADE,
    INDEX idx_songid (songid),
    INDEX idx_rating (rating)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Table: genre_requests
-- Stores user requests for new genres
-- =============================================
CREATE TABLE IF NOT EXISTS genre_requests (
    requestid INT AUTO_INCREMENT PRIMARY KEY,
    userid INT NOT NULL,
    genre_name VARCHAR(50) NOT NULL,
    requested_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) DEFAULT 'pending',  -- pending, approved, rejected
    reviewed_by INT NULL,
    reviewed_date DATETIME NULL,
    FOREIGN KEY (userid) REFERENCES users(userid) ON DELETE CASCADE,
    FOREIGN KEY (reviewed_by) REFERENCES users(userid) ON DELETE SET NULL,
    INDEX idx_status (status),
    INDEX idx_userid (userid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- Insert Default Data
-- =============================================

-- Insert default admin user (password: admin123)
-- Password is SHA256 hashed
INSERT INTO users (username, password, email, IsAdmin) VALUES
('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'admin@aurora.com', 1)
ON DUPLICATE KEY UPDATE username=username;

-- Insert default genres
INSERT INTO genres (name) VALUES
('Rock'),
('Pop'),
('Jazz'),
('Classical'),
('Electronic'),
('Hip Hop'),
('Country'),
('Blues'),
('Reggae'),
('Metal'),
('Folk'),
('R&B'),
('Soul'),
('Funk'),
('Disco'),
('Techno'),
('House'),
('Trance'),
('Dubstep'),
('Ambient'),
('Indie'),
('Alternative'),
('Punk'),
('Grunge'),
('Ska'),
('Gospel'),
('Latin'),
('World'),
('Experimental'),
('Lo-Fi')
ON DUPLICATE KEY UPDATE name=name;

-- =============================================
-- Verification Queries
-- =============================================
-- Run these to verify setup:

-- Check all tables exist
SHOW TABLES;

-- Check user count
SELECT COUNT(*) as user_count FROM users;

-- Check genre count
SELECT COUNT(*) as genre_count FROM genres;

-- List all genres
SELECT * FROM genres ORDER BY name;

-- =============================================
-- Useful Queries for Development
-- =============================================

-- Get site statistics
SELECT 
    (SELECT COUNT(*) FROM users) as total_users,
    (SELECT COUNT(*) FROM songs) as total_songs,
    (SELECT COUNT(*) FROM playlists) as total_playlists,
    (SELECT COALESCE(SUM(plays), 0) FROM songs) as total_plays;

-- Get top 10 most played songs
SELECT s.songid, s.title, s.plays, u.username as uploader
FROM songs s
JOIN users u ON s.userid = u.userid
ORDER BY s.plays DESC
LIMIT 10;

-- Get songs with their genres
SELECT s.title, GROUP_CONCAT(g.name SEPARATOR ', ') as genres
FROM songs s
LEFT JOIN song_genres sg ON s.songid = sg.songid
LEFT JOIN genres g ON sg.genreid = g.genreid
GROUP BY s.songid, s.title;

-- Get user statistics
SELECT 
    u.username,
    COUNT(DISTINCT s.songid) as songs_uploaded,
    COALESCE(SUM(s.plays), 0) as total_plays,
    COUNT(DISTINCT p.playlistid) as playlists_created,
    COUNT(DISTINCT r.ratingid) as ratings_given
FROM users u
LEFT JOIN songs s ON u.userid = s.userid
LEFT JOIN playlists p ON u.userid = p.userid
LEFT JOIN ratings r ON u.userid = r.userid
GROUP BY u.userid, u.username;

-- =============================================
-- Maintenance Queries
-- =============================================

-- Reset all play counts (for testing)
-- UPDATE songs SET plays = 0;

-- Delete all songs (for testing)
-- DELETE FROM songs;

-- Delete all playlists (for testing)
-- DELETE FROM playlists;

-- =============================================
-- End of Setup Script
-- =============================================
