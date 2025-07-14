create database cellphones_db;

use cellphones_db
-- xoá cột
alter table store drop column city_id
-- thêm cột
alter table users add password text not null
--thêm khoá ngoại
ALTER TABLE cart
ADD CONSTRAINT fk_cart_user
  FOREIGN KEY (user_id)
  REFERENCES users(id)
-- đánh index
CREATE INDEX index_user
ON users (username);

create table order_item(
  id bigint primary key identity,
  order_id bigint not null,
  product_id bigint not null,
  store_id bigint not null,
  quantity int CHECK(quantity > 0) not null,
  price DECIMAL(18,2) NOT NULL,
  shipping_fee DECIMAL(18,2) DEFAULT 0,
  status nvarchar(255) not null,
  status_del bit null,
  create_by bigint null,
  create_date datetime not null,
  update_by bigint null,
  update_date datetime null,
)

	