# Home Elixir — Proyecto Integrador Final

**Sitio web de perfumería desarrollado con ASP.NET Core MVC y MySQL**

## Descripción general

**Home Elixir** es una aplicación web de comercio electrónico orientada a la venta de perfumes y fragancias. El sistema permite administrar el catálogo de productos, consultar marcas y categorías, gestionar cuentas de usuario y direcciones, agregar productos al carrito y realizar órdenes de compra.

El proyecto busca centralizar el proceso de compra y la administración de una perfumería mediante una aplicación web con persistencia en base de datos, autenticación, autorización por roles y gestión de las operaciones comerciales.

Está desarrollado con una arquitectura MVC, separando modelos, vistas y controladores para organizar la lógica del sistema y facilitar su mantenimiento.

## Tecnologías utilizadas

- **Backend:** C# y ASP.NET Core MVC.
- **Persistencia:** Entity Framework Core.
- **Base de datos:** MySQL.
- **Frontend:** HTML, CSS, JavaScript, Razor y Bootstrap.
- **Autenticación:** autenticación mediante cookies.
- **Control de versiones:** Git y GitHub.

## Funcionalidades principales

### Usuarios y seguridad
- Registro e inicio de sesión.
- Cierre de sesión.
- Roles de usuario: `Cliente` y `Administrador`.
- Restricción de funcionalidades según el rol.
- Perfil de usuario con avatar personalizado.

### Catálogo de productos
- Listado y consulta de productos.
- Búsqueda de productos por nombre y marca.
- Filtros por marca y categoría.
- Alta, modificación y eliminación de productos para usuarios administradores.
- Gestión de stock.
- Asociación de productos con marcas y categorías.
- Almacenamiento de imágenes de productos en el servidor.

### Compras y órdenes
- Carrito de compras para clientes autenticados.
- Incorporación de productos y actualización de cantidades.
- Validación de stock.
- Gestión de direcciones de entrega.
- Generación y consulta de órdenes.
- Consulta de los detalles de una orden.
- Actualización del estado de las órdenes por parte del administrador.

### Pagos
- Registro y consulta de información de pagos asociados a órdenes.
- Gestión administrativa de pagos y sus estados, según las funcionalidades implementadas.

### Administración
- Gestión de marcas y categorías.
- Gestión de productos y stock.
- Consulta de órdenes y pagos.
- Listados con paginación del lado del servidor en los módulos que la tienen implementada.

##  Modelo de datos

La aplicación utiliza entidades relacionadas mediante Entity Framework Core y una base de datos MySQL.

Entre las principales entidades se encuentran:

| Entidad | Responsabilidad |
|---|---|
| `Usuario` | Almacena los datos de las cuentas, el rol y la información del perfil. |
| `Direccion` | Registra las direcciones asociadas a los usuarios. |
| `Producto` | Contiene la información de los perfumes, sus precios, stock e imágenes. |
| `Marca` | Representa las marcas del catálogo. |
| `Categoria` | Clasifica los productos. |
| `Carrito` | Representa el carrito de compra de un usuario. |
| `ItemCarrito` | Registra los productos y las cantidades del carrito. |
| `Orden` | Almacena la información de cada compra. |
| `DetalleOrden` | Relaciona una orden con los productos comprados y sus cantidades y precios. |
| `Pago` | Registra la información del pago asociado a una orden. |

### Relaciones principales

- **Usuario – Direccion:** un usuario puede tener varias direcciones.
- **Usuario – Orden:** un usuario puede realizar varias órdenes.
- **Marca – Producto:** una marca puede estar asociada a varios productos.
- **Categoria – Producto:** una categoría puede contener varios productos.
- **Orden – DetalleOrden:** una orden puede contener varios detalles.
- **Producto – DetalleOrden:** un producto puede aparecer en varios detalles de órdenes.
- **Orden – Producto:** relación de muchos a muchos resuelta mediante la entidad intermedia `DetalleOrden`.
- **Orden – Pago:** el pago se asocia a una orden.


##  Seguridad y roles

La aplicación diferencia dos roles principales:

- **Cliente:** puede acceder a las funcionalidades de compra, administrar sus direcciones y consultar sus propias órdenes.
- **Administrador:** puede acceder a las funcionalidades de gestión del catálogo y a las operaciones administrativas habilitadas.

## Instalación y ejecución local

### Requisitos previos

- .NET SDK compatible con el proyecto.
- MySQL Server.
- MySQL Workbench, opcional para administrar la base de datos.
- Visual Studio o un editor compatible con .NET.
- Git.

### 1. Clonar el repositorio

```bash
git clone URL_DEL_REPOSITORIO
cd ProyectoIntegradorFinal
```

### 2. Configurar la base de datos

Crear la base de datos `perfumeriadb` en MySQL e importar el script SQL incluido en el repositorio, cuando esté incorporado.

El archivo de conexión debe configurarse con los datos del servidor local. No publicar contraseñas reales ni credenciales privadas en el repositorio.

### 3. Configurar la conexión

Revisar `appsettings.json` y configurar la cadena de conexión correspondiente a MySQL.

Ejemplo orientativo:

```json
{
  "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Port=3306;Database=PerfumeriaDb;Uid=root;Pwd=CONTRASENA_LOCAL;"
  }
}
```

Los nombres de la cadena de conexión y del proveedor deben coincidir con la configuración real del proyecto.


## Matriz de evidencias de los requerimientos mínimos

La siguiente matriz identifica el estado de cada requisito y la evidencia que debe incluirse para su evaluación. Los estados deberán actualizarse cuando cada funcionalidad esté terminada y verificada.

| Requerimiento | Estado | Enlace al código |
|---|---|---|---|
| Modelo de datos: al menos 6 entidades relacionadas, relación 1:N y relación N:M o entidad intermedia | 🟢 Implementado | `Models/`, `Data/AppDbContext.cs` | 
| Login y Logout | 🟢 Implementado | `Controllers/UsuariosController.cs` y configuración de autenticación | 
| Roles y autorización en backend | 🟢Implementados| Controladores con `[Authorize]` |
| Avatar personalizado | 🟢Implementado | `Controllers/UsuariosController.cs`, vistas de perfil y `wwwroot/images/avatares/` |
| Manejo de archivos adicional al avatar | 🟢 Carga de imágenes de productos implementada; verificar validaciones y almacenamiento | `Controllers/ProductosController.cs`, `wwwroot/images/` |
| ABM completo con Vue.js u otro framework reactivo mediante AJAX/JSON, sin recarga | 🔴 Pendiente de verificar e implementar | Incorporar rutas y componentes del ABM cuando estén desarrollados |
| Paginación del lado del servidor en los listados principales | 🟢 Implementada en varios listados principales | Controladores y vistas de Productos, Marcas, Categorías, Órdenes y Pagos, según corresponda | 
| Búsqueda asíncrona de entidades relacionadas mediante AJAX en el servidor | 🟢Implementado | Incorporar el endpoint de búsqueda y el formulario correspondiente |
| Web API REST protegida mediante JWT | 🟢 Implementado | Incorporar controlador de autenticación y endpoints protegidos |
| 



## Datos y cuentas de prueba

Las credenciales se documentarán después de crear y comprobar las cuentas de prueba en la base de datos.

| Rol | Usuario de prueba | Contraseña de prueba |
|---|---|---|
| Administrador | admin@homeelixir.com | Admin1234|
| Cliente | Pendiente de configurar | Pendiente de configurar |


## Estructura del repositorio

```text
ProyectoIntegradorFinal/
├──ApiControllers/
├── Controllers/
├── Data/
├── Models/
├── Views/
├──ViewModels/
├── wwwroot/
│   └── images/
├── appsettings.json
├── .gitignore
└── README.md
```

##  Proyecto Integrador Final

**Proyecto:** Home Elixir  
**Tipo:** Aplicación web MVC de comercio electrónico  
**Tecnologías principales:** ASP.NET Core MVC, C#, Entity Framework Core y MySQL.
