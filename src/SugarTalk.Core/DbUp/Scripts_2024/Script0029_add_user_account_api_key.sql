create table if not exists `user_account_api_key`
(
    `id` int primary key auto_increment,
    `user_account_id` int not null,
    `api_key` varchar(128) not null,
    `description` varchar(256) null
)