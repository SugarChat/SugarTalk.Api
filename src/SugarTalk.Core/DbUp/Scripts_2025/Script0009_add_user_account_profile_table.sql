create table if not exists `user_account_profile`
(
    `id` int primary key auto_increment,
    `user_account_id` int not null,
    `url` varchar(500) not null,
    `created_date` datetime(3) not null
)charset=utf8mb4;