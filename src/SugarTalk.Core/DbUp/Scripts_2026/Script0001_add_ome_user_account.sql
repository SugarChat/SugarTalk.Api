create table if not exists `ome_user_account` (
    `id` char(36) NOT NULL,
    `user_name` varchar(255) NOT NULL,
    `nick_name` varchar(255) NULL,
    `created_way` varchar(255) NOT NULL,
    `expire_time` int NOT NULL,
    `aud` text NOT NULL,
    `created_time` datetime(3) NOT NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;