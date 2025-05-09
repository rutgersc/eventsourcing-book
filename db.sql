﻿CREATE TABLE "Carts" (
    "CartId" uuid NOT NULL,
    CONSTRAINT "PK_Carts" PRIMARY KEY ("CartId")
);


CREATE TABLE "CartsWithProducts" (
    "CartId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    CONSTRAINT "PK_CartsWithProducts" PRIMARY KEY ("CartId", "ProductId")
);


CREATE TABLE "Inventories" (
    "ProductId" uuid NOT NULL,
    "Inventory" integer NOT NULL,
    CONSTRAINT "PK_Inventories" PRIMARY KEY ("ProductId")
);


CREATE TABLE "InventoriesReadModel" (
    "ProductId" uuid NOT NULL,
    "Inventory" integer NOT NULL
);


CREATE TABLE "Pricing" (
    "ProductId" uuid NOT NULL,
    "Price" numeric NOT NULL,
    CONSTRAINT "PK_Pricing" PRIMARY KEY ("ProductId")
);


CREATE TABLE "CartItem" (
    "CartItemId" uuid NOT NULL,
    "ItemId" uuid NOT NULL,
    "CartId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    CONSTRAINT "PK_CartItem" PRIMARY KEY ("CartItemId"),
    CONSTRAINT "FK_CartItem_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("CartId") ON DELETE CASCADE
);


CREATE INDEX "IX_CartItem_CartId" ON "CartItem" ("CartId");


