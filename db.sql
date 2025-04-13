CREATE TABLE "Carts" (
    "CartId" uuid NOT NULL,
    CONSTRAINT "PK_Carts" PRIMARY KEY ("CartId")
);


CREATE TABLE "Inventories" (
    "ProductId" uuid NOT NULL,
    "Inventory" integer NOT NULL,
    CONSTRAINT "PK_Inventories" PRIMARY KEY ("ProductId")
);


CREATE TABLE "CartItem" (
    "CartItemId" uuid NOT NULL,
    "CartId" uuid NOT NULL,
    CONSTRAINT "PK_CartItem" PRIMARY KEY ("CartItemId"),
    CONSTRAINT "FK_CartItem_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("CartId") ON DELETE CASCADE
);


CREATE INDEX "IX_CartItem_CartId" ON "CartItem" ("CartId");


