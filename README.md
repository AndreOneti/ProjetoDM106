# ProjetoDM106
<br>
ADMIN<br>
Usuario                   Password<br>
andreluiz@gea.inatel.br   Matilde#7<br>
<br><br>
USER<br>
Usuario                   Password<br>
matilde@siecola.com      Matilde#7<br>

Metodos do projeto.

    Metodos do pedido.

        ->// GET: api/Order/getfrete?id=1
            -> necessario passar o id do pedido
            -> Utiliza os metodos GET: api/Order/cep e api/Order/frete
            
        ->// GET: api/Order/cep?email=email
            ->Necessario pasa o E-mail
            -> Busca o cep do usuario apartir do seu email
        
        ->// GET: api/Order/frete?id=1&cep=37540000&peso=20&comprimento=20&altura=20&largura=20&diametro=20&preco=20
            -> Busca nos correios o valor do frete e o tempo de entrega
        
        ->// GET: api/Order/byemail?email=email
            -> Busca o pedido apartir de um E-mail
        
        ->// POST: api/Orders/recover?id=5
            -> Seta o status do pedido para "novo"

        ->// GET: api/Orders
            -> Busca todos os pedidos
        
        ->// GET: api/Orders/5
            -> Busca um unico pedido

        ->// PUT: api/Orders/5
            -> Altera um pedido

        ->// POST: api/Orders
            -> Cria um novo pedido

        ->// POST: api/Orders/5
            -> Fecha um pedido

        ->// DELETE: api/Orders/5
            -> Deleta um pedido

    Metodos do produto.
    
        ->// GET: api/Products
            -> Lista todos os produtos

        -> // GET: api/Products/5
            -> Lista um unico produto apartir id

        ->// PUT: api/Products/5
            -> Altera um produto apartir id

        ->// POST: api/Products
            -> Cria um produto

        ->// DELETE: api/Products/5
            -> Deleta um produto apartir do id
