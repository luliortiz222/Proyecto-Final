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

El diagrama entidad-relación se incorporará al repositorio en `docs/der.png`.

> **Nota:** el diagrama debe corresponder a las relaciones reales definidas en los modelos y en la base de datos del proyecto.

##  Seguridad y roles

La aplicación diferencia dos roles principales:

- **Cliente:** puede acceder a las funcionalidades de compra, administrar sus direcciones y consultar sus propias órdenes.
- **Administrador:** puede acceder a las funcionalidades de gestión del catálogo y a las operaciones administrativas habilitadas.

Las acciones protegidas utilizan autorización en el backend para evitar que las restricciones dependan únicamente de los elementos visibles en la interfaz.

Las contraseñas deben almacenarse mediante hash seguro, nunca en texto plano.

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

### 4. Restaurar dependencias

Desde la carpeta que contiene el archivo `.csproj`:

```bash
dotnet restore
```

### 5. Ejecutar la aplicación

```bash
dotnet run
```

La aplicación estará disponible en la dirección local que indique la consola o la configuración de lanzamiento de Visual Studio.

## Matriz de evidencias de los requerimientos mínimos

La siguiente matriz identifica el estado de cada requisito y la evidencia que debe incluirse para su evaluación. Los estados deberán actualizarse cuando cada funcionalidad esté terminada y verificada.

| Requerimiento | Estado | Enlace al código | Evidencia |
|---|---|---|---|
| Modelo de datos: al menos 6 entidades relacionadas, relación 1:N y relación N:M o entidad intermedia | 🟡 Implementado en gran parte; verificar el esquema final | `Models/`, `Data/AppDbContext.cs` | `docs/der.png` — pendiente de incorporar |
| Login y Logout | 🟡 Implementados; verificar el flujo completo | `Controllers/UsuariosController.cs` y configuración de autenticación | Capturas de inicio y cierre de sesión |
| Roles y autorización en backend | 🟡 Implementados; verificar permisos de Cliente y Administrador | Controladores con `[Authorize]` | Capturas de funcionalidades según rol |
| Avatar personalizado | 🟡 Implementado; verificar carga, visualización y validaciones | `Controllers/UsuariosController.cs`, vistas de perfil y `wwwroot/images/avatares/` | Captura del perfil con avatar |
| Manejo de archivos adicional al avatar | 🟡 Carga de imágenes de productos implementada; verificar validaciones y almacenamiento | `Controllers/ProductosController.cs`, `wwwroot/images/` | Captura de alta o edición de un producto con imagen |
| ABM completo con Vue.js u otro framework reactivo mediante AJAX/JSON, sin recarga | 🔴 Pendiente de verificar e implementar | Incorporar rutas y componentes del ABM cuando estén desarrollados | GIF o video del ABM dinámico |
| Paginación del lado del servidor en los listados principales | 🟡 Implementada en varios listados; revisar todos los módulos principales | Controladores y vistas de Productos, Marcas, Categorías, Órdenes y Pagos, según corresponda | Capturas de distintas páginas y evidencia de consultas paginadas |
| Búsqueda asíncrona de entidades relacionadas mediante AJAX en el servidor | 🔴 Pendiente de verificar e implementar | Incorporar el endpoint de búsqueda y el formulario correspondiente | GIF de búsqueda de entidades relacionadas |
| Web API REST protegida mediante JWT | 🔴 Pendiente de verificar e implementar en Home Elixir | Incorporar controlador de autenticación y endpoints protegidos | Colección de Postman y captura de una petición con Bearer Token |
| Script SQL con datos de prueba | 🟡 Existe un dump de MySQL; verificar que incluya esquema y datos actualizados | `docs/` o carpeta de scripts SQL | Importación de la base de datos de prueba |
| Credenciales de prueba para los roles | 🔴 Pendiente de documentar y verificar | Sección de credenciales de este README | Prueba de acceso con cada rol |

**Referencias de código:** los nombres de carpetas y controladores son rutas relativas al proyecto. Los enlaces directos a GitHub deben agregarse una vez definido el repositorio público.

## Datos y cuentas de prueba

Las credenciales se documentarán después de crear y comprobar las cuentas de prueba en la base de datos.

| Rol | Usuario de prueba | Contraseña de prueba |
|---|---|---|
| Administrador | Pendiente de configurar | Pendiente de configurar |
| Cliente | Pendiente de configurar | Pendiente de configurar |

No utilizar cuentas personales ni contraseñas reales. Las cuentas de prueba deben estar preparadas exclusivamente para la evaluación.

## Estructura del repositorio

```text
ProyectoIntegradorFinal/
├── Controllers/
├── Data/
├── Models/
├── Views/
├── wwwroot/
│   └── images/
├── docs/
│   └── der.png
├── appsettings.json
├── .gitignore
└── README.md
```

La estructura es orientativa y debe ajustarse a los directorios reales del proyecto. Los archivos de configuración que contengan secretos no deben publicarse.

##  Estado de entrega

El proyecto cuenta con una base funcional de comercio electrónico, persistencia en MySQL, autenticación, roles y distintos módulos de gestión.

Antes de considerar la entrega completa para promoción, se debe verificar y completar la matriz de evidencias, incorporar el diagrama DER, asegurar la ejecución local desde cero, incluir el script SQL actualizado con datos de prueba y resolver los requisitos que todavía estén pendientes, especialmente el ABM reactivo con AJAX/JSON, la búsqueda asíncrona de entidades relacionadas y la API REST con JWT.

##  Proyecto Integrador Final

**Proyecto:** Home Elixir  
**Tipo:** Aplicación web MVC de comercio electrónico  
**Tecnologías principales:** ASP.NET Core MVC, C#, Entity Framework Core y MySQL.
