using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMapper;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using ApiPeliculas.Migrations;
using ApiPeliculas.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Agregando el contexto a las dependencias y declarando que tipo de BD utilizará
builder.Services.AddDbContext<ApplicationDbContext>(options =>
                                                   options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

//Soporte para autenticación con .NET Identity
builder.Services.AddIdentity<AppUsuario, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

//Soporte para cache
builder.Services.AddResponseCaching();

//Agregamos los repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

//Traemos la key desde el appsettings para utilizarla para la autenticación
var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//Soporte para versionamiento
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true; //Asumiendo la versión por defecto de la api cuando no este especificada
    options.DefaultApiVersion = new ApiVersion(1,0); //Versión de la api
    options.ReportApiVersions = true;
    //options.ApiVersionReader = ApiVersionReader.Combine(
    //    new QueryStringApiVersionReader("api-version") //?api-version=1.0
    //    //new HeaderApiVersionReader("X-Version"),
    //    //new MediaTypeApiVersionReader("ver"),
    //);
});

apiVersioningBuilder.AddApiExplorer(
        options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true; // Nos sirve para pasar las urls de forma dinámica por el controlador
        }
    );

//Agregamos el AutoMapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));

//Aquí se configura la utenticación
builder.Services.AddAuthentication
    //Expresión lamda que establece el esquema de autenticación predetermindado que en este caso es JWT
    (x =>
        {
            //Agregar los servicios de autenticación a la aplicación
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(x => //Cuando se requiera autenticación utilizará el siguiente esquema para el cliente
    {
        x.RequireHttpsMetadata = false;//En producción debemos pasar esto a true para que requiera los metadatos solo con https
        x.SaveToken = true; //El token de debe guardar una vez validado
        x.TokenValidationParameters = new TokenValidationParameters //Establece los parámetros de validación del token
        {
            ValidateIssuerSigningKey = true, //La clave de firma del emisor debe ser validada
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), //Establece la clave de firma del emisor que se usará para validar la firma del token debe ser la misma clave que se uso para crear los tokens
            ValidateIssuer = false, //Indica que no se debe validar el emisor del token
            ValidateAudience = false //Indica que no se debe validar el la audiencia     del token
        };
    });

//En esta sección podemos agregar los servicios necesarios a los controladores de manera global
builder.Services.AddControllers(options => //Aquí podemos añadir el soporte para cache global
{
    //Cache profile. un cache global y a así no tener que ponerlo en todas partes
    options.CacheProfiles.Add("CachePorDefecto", new CacheProfile()
    {
        Duration = 20
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//Configuración de Swagger
/*
* Este método agrega o configura los servicios de Swagger en el contenedor
* de servicios de la aplicación
*/
builder.Services.AddSwaggerGen(options =>
    {
        //Definimos el tipo de autorización y sus propiedades, debe ser la misma que utilizamos para los tokens
        /*
         * Este método define una nueva forma de seguridad en Swagger.
         * En este caso basa en JWT
         */
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = 
            "Autenticación JWT usando el esquema Bearer. \r\n\r\n " +
            "Ingresa la palabra 'Bearer' seguido de un [espacio] y después su token en el campo de abajo. \r\n\r\n" +
            "Ejemplo: \"Bearer tkjkkkahss\"",
            Name = "Authorization",
            In = ParameterLocation.Header, //Aquí establece en donde estará almacenado el parametro de autorización
            Scheme = "Bearer" //Define el esquema de autenticación
        });

        //Para permitir implementar la autenticación
        /*
         * Agrega un requisito de seguridad a swagger, indicando que los sitemas de seguridad definido es
         * necesario para acceder a los endpoints de la API
         */
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                //Aquí especificamos los requisitos de seguridad
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()//Una lista vacía que puede ser utilizada para especificar los alcances o los scope requridos por la seguridad
            }
        });

        //Configuración de documentación de la API por versiones
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1.0",
            Title = "Peliculas Api v1",
            Description = "Api de películas versión #1",
            TermsOfService = new Uri("https://www.instagram.com/franyi_liriano/"),
            Contact = new OpenApiContact
            {
                Name = "Franyi Liriano",
                Url = new Uri("https://www.instagram.com/franyi_liriano/")
            },
            License = new OpenApiLicense
            {
                Name = "Licencia Personal",
                Url = new Uri("https://www.instagram.com/franyi_liriano/")
            },

        });
        options.SwaggerDoc("v2", new OpenApiInfo
        {
            Version = "v2.0",
            Title = "Peliculas Api v2",
            Description = "Api de películas versión #2  ",
            TermsOfService = new Uri("https://www.instagram.com/franyi_liriano/"),
            Contact = new OpenApiContact
            {
                Name = "Franyi Liriano",
                Url = new Uri("https://www.instagram.com/franyi_liriano/")
            },
            License = new OpenApiLicense
            {
                Name = "Licencia Personal",
                Url = new Uri("https://www.instagram.com/franyi_liriano/")
            },

        });
    }   
);

/*Soporte para CORS
Se pueden habilitar: 1-Un dominio, 2-multiples dominios separados por comas
3- cualquier dominio con el * (tener en cuenta la seguridad)
Se usa (*) para todos los dominios
*/

//Incluyendo CORS
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors",build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //Para personalizar la documentación de la API
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json","ApiPeliculasV1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json","ApiPeliculasV2");
    });
}
//Soporte para archivos estáticos como imágenes
app.UseStaticFiles();

app.UseHttpsRedirection();

//Soporte para CORS
//Dentro del método UseCors va el nombre de la politica de CORS que declaramos anteriormente en el builder.
app.UseCors("PoliticaCors");
//Soporte para la autentición
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
