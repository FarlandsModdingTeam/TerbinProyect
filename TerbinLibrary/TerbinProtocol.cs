using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;

namespace TerbinLibrary;

public class TerbinProtocol
{
    public const ushort MAX_PLD = 0xFFF; // ¡Vamos Miura!
    public const ushort FRAGMENT_IN = 0xFFF;
    public const double FRAGMENT_IN__MULTIPLICATE_INVERSE = 1.0D / FRAGMENT_IN;

    public const ushort ORDER_SINGLE = ushort.MinValue;

    public const ushort FIRST_PACKET = 1;
    public const ushort FINAL_PACKET = ushort.MaxValue;

    public const ushort MAXIMUS_RESPONSE_TIME = 4; // ¿Se necesita un short? // 180

    public const byte RESERVE_PROTOCOL = 9;
    public const byte RESERVE_MEMORY = 9;


    public static async Task InitProtocol(CancellationToken pTokenCancellation)
    {
        await autoCreatePipe(pTokenCancellation);
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }

    private static async Task autoCreatePipe(CancellationToken pTokenCancellation)
    {
        try
        {
            var communicator = new TerbinCommunicator(true, pTokenCancellation);
            communicator.OnRecive += ExecutableDispatcher.DispatchAsync;
            communicator.OnNewClientConnect += async () =>
            {
                _ = Task.Run(() => autoCreatePipe(pTokenCancellation), pTokenCancellation);
            };
            var executor = new TerbinExecutor();
        }
        catch(Exception e)
        {
            Console.WriteLine($"[Worker] Error-> {e.Message}");
        }
    }
}

[Flags]
public enum CodeTypeData : byte
{
    Singles = 0b0000000_0,
    Array   = 0b0000000_1,

    Byte    = 0b0000000_0,
    sByte   = 0b0000001_0,
    Short   = 0b0000010_0,
    uShort  = 0b0000011_0,
    Int     = 0b0000100_0,
    uInt    = 0b0000101_0,
    Long    = 0b0000110_0,
    uLong   = 0b0000111_0,

    Float   = 0b0001000_0,
    Doueble = 0b0001001_0,
    Decimal = 0b0001010_0,

    Char    = 0b0001011_0,
}

/// <summary>
/// 0 -> 9 | 
/// Reserved the first 10.
/// </summary>
public enum CodeTerbinProtocol : byte
{
    Stop = 0,
    None = 1,
    Load = 2,
    Cancel = 3,
    Solicit = 4,
    Info = 5,

    // Si puede ayudar a ahorrarte fuciones.
    // C.R.U.D for you: 
    Create = TerbinCRUD.Create,
    Read = TerbinCRUD.Read,
    Update = TerbinCRUD.Update,
    Deleted = TerbinCRUD.Deleted,
}

public enum TerbinCRUD : byte
{
    Create = 6,
    Read = 7,
    Update = 8,
    Deleted = 9,
}

public enum CodeTerbinMemory : byte
{
    None = 0,
    NotAsign = 1,
    New = 2,
    Undefined = 3, // Literaly

    Undefined4 = 4,
    Undefined5 = 5,
    Undefined6 = 6,
    Undefined7 = 7,
    Undefined8 = 8,
    Undefined9 = 9,
}

/* -- Parecida a HTTP pero no la voy a seguir a raja tabla.
    1xx → Informativos
    2xx → Éxito (ej. 200 OK)
    3xx → Redirecciones
    4xx → Error del cliente (ej. 404 Not Found)
    5xx → Error del worker (ej. 500 Internal Server Error)
 */
public enum CodeStatus : short
{
    NotAsign = -1,

    Succes = 200,

    Execute = 300, // Hace mucha falta.
    Info = 301,

    BadRequest = 400,
    NotFound = 404,

    ActionNotFound = 440,


    GenericWorkerError = 500,
    ExecutionError = 501,
    ErrorNotPayload = 502,
    SerializeError = 503,
    AccesNullOrNotExist = 504,

    OverMaximumTime = 555,
}












// ==========================================
// PRUEBAS: Para intentar hacer un enum que sustituya a las Excepciones
// ==========================================
[Flags]
public enum ExceptionFlags : ulong
{
    None = 0,

    // --- Base & Runtime ---
    Exception = 1ul << 0,
    SystemException = 1ul << 1,
    ApplicationException = 1ul << 2,
    NullReferenceException = 1ul << 3,
    InvalidOperationException = 1ul << 4,
    NotImplementedException = 1ul << 5,
    NotSupportedException = 1ul << 6,
    PlatformNotSupportedException = 1ul << 7,

    // --- Argumentos ---
    ArgumentException = 1ul << 8,
    ArgumentNullException = 1ul << 9,
    ArgumentOutOfRangeException = 1ul << 10,

    // --- Memoria y Ejecución Fatal ---
    OutOfMemoryException = 1ul << 11,
    StackOverflowException = 1ul << 12,
    ExecutionEngineException = 1ul << 13,
    AccessViolationException = 1ul << 14,

    // --- Matemáticas y Tipos ---
    DivideByZeroException = 1ul << 15,
    OverflowException = 1ul << 16,
    ArithmeticException = 1ul << 17,
    FormatException = 1ul << 18,
    InvalidCastException = 1ul << 19,
    IndexOutOfRangeException = 1ul << 20,

    // --- Tareas y Multihilos ---
    TimeoutException = 1ul << 21,
    OperationCanceledException = 1ul << 22,
    TaskCanceledException = 1ul << 23,
    AggregateException = 1ul << 24,
    ThreadAbortException = 1ul << 25,
    ThreadInterruptedException = 1ul << 26,
    ThreadStateException = 1ul << 27,

    // --- Archivos e IO ---
    IOException = 1ul << 28,
    FileNotFoundException = 1ul << 29,
    DirectoryNotFoundException = 1ul << 30,
    PathTooLongException = 1ul << 31,

    // NOTA: A partir de aquí superamos los 32 bits. 
    // Es OBLIGATORIO usar el sufijo 'ul' (unsigned long) o el compilador fallará.
    EndOfStreamException = 1ul << 32,
    InvalidDataException = 1ul << 33,

    // --- Red y Web ---
    SocketException = 1ul << 34,
    HttpRequestException = 1ul << 35,
    WebException = 1ul << 36,

    // --- Base de Datos y Datos ---
    DataException = 1ul << 37,
    DBConcurrencyException = 1ul << 38,
    SqlException = 1ul << 39, // Si usas System.Data.SqlClient

    // --- Seguridad y Acceso ---
    UnauthorizedAccessException = 1ul << 40,
    SecurityException = 1ul << 41,
    CryptographicException = 1ul << 42,

    // --- Colecciones ---
    KeyNotFoundException = 1ul << 43,
    RankException = 1ul << 44,
    ArrayTypeMismatchException = 1ul << 45,

    // --- Serialización y Formatos ---
    SerializationException = 1ul << 46,
    XmlException = 1ul << 47,
    JsonException = 1ul << 48,

    // --- Reflexión y Carga de Tipos ---
    TypeInitializationException = 1ul << 49,
    TypeLoadException = 1ul << 50,
    MissingMemberException = 1ul << 51,
    MissingFieldException = 1ul << 52,
    MissingMethodException = 1ul << 53,
    AmbiguousMatchException = 1ul << 54,
    BadImageFormatException = 1ul << 55,

    // --- Excepciones raras de bajo nivel ---
    InvalidProgramException = 1ul << 56,
    AppDomainUnloadedException = 1ul << 57,
    CannotUnloadAppDomainException = 1ul << 58,
    ContextMarshalException = 1ul << 59,
    DuplicateWaitObjectException = 1ul << 60,
    FieldAccessException = 1ul << 61,
    MethodAccessException = 1ul << 62,

    // EL ÚLTIMO BIT DISPONIBLE (Bit 63)
    DataMisalignedException = 1ul << 63
}



[Flags]
public enum ErrorFlags : ulong
{
    None = 0,

    // --- 1. Validaciones Básicas de Datos (Bits 0-7) ---
    NullParameter = 1ul << 0,
    EmptyString = 1ul << 1,
    FormatInvalid = 1ul << 2,
    ValueOutOfRange = 1ul << 3,
    InvalidLength = 1ul << 4,
    MissingRequiredData = 1ul << 5,
    InvalidCast = 1ul << 6,
    // Bit 7 libre para el futuro

    // --- 2. Errores de Protocolo y Paquetes (Bits 8-15) ---
    // Ideales para usar al leer un PacketRequest o Header
    PacketMalformed = 1ul << 8,
    HeaderInvalid = 1ul << 9,
    PayloadTooLarge = 1ul << 10, // Ej: Supera MAX_PLD (0xFFF)
    FragmentationError = 1ul << 11, // Ej: Error al juntar fragmentos
    OrderMismatch = 1ul << 12, // Ej: Esperabas FIRST_PACKET y llegó otro
    ChecksumMismatch = 1ul << 13,
    UnknownProtocolType = 1ul << 14,
    // Bit 15 libre

    // --- 3. Memoria y Streams (Bits 16-23) ---
    // Ideales para TerbinMemory y MemoryStream
    MemoryNotAllocated = 1ul << 16,
    MemoryIdNotFound = 1ul << 17, // Cuando Release() o TryGetResult() fallan
    BufferOverflow = 1ul << 18,
    StreamReadError = 1ul << 19,
    StreamWriteError = 1ul << 20,
    EndOfStreamReached = 1ul << 21,
    MemoryCorrupted = 1ul << 22,
    // Bit 23 libre

    // --- 4. Ejecución y Dispatching (Bits 24-31) ---
    // Ideales para ExecutableDispatcher
    ActionNotFound = 1ul << 24, // Equivalente a tu CodeStatus.ActionNotFound
    HandlerNotSet = 1ul << 25,
    MethodMismatch = 1ul << 26, // Si la firma del método no cuadra
    ExecutionFault = 1ul << 27, // Si el catch genérico en DispatchAsync salta
    ActionAlreadyExists = 1ul << 28, // Si intentas registrar un Action repetido
    // Bits 29-31 libres

    // --- 5. Estado y Lógica de Negocio (Bits 32-41) ---
    NotFound = 1ul << 32,
    AlreadyExists = 1ul << 33,
    InvalidState = 1ul << 34,
    LimitExceeded = 1ul << 35,
    DuplicateRequest = 1ul << 36,
    NotImplementedYet = 1ul << 37,
    OperationRejected = 1ul << 38,
    // Bits 39-41 libres

    // --- 6. Autenticación y Seguridad (Bits 42-47) ---
    Unauthorized = 1ul << 42,
    Forbidden = 1ul << 43,
    TokenExpired = 1ul << 44,
    SignatureInvalid = 1ul << 45,
    // Bits 46-47 libres

    // --- 7. Tiempos, Red y Concurrencia (Bits 48-55) ---
    Timeout = 1ul << 48, // Ej: Supera MAXIMUS_RESPONSE_TIME
    ResourceLocked = 1ul << 49,
    NetworkUnavailable = 1ul << 50,
    ConnectionLost = 1ul << 51,
    UserCancelled = 1ul << 52,
    RateLimited = 1ul << 53,
    // Bits 54-55 libres

    // --- 8. Errores Críticos de Infraestructura (Bits 56-63) ---
    InternalSystemError = 1ul << 56,
    DiskWriteFailed = 1ul << 57,
    DataCorrupted = 1ul << 58,

    // El bit más alto (63) como error fatal insalvable
    FatalError = 1ul << 63
}

// Usamos ushort (0 a 65,535) para que ocupe solo 2 bytes si lo mandas por red.
public enum TerbinErrorCode : ushort
{
    None = 0, // Todo correcto (Success)

    // ==========================================
    // 1000 - 1999: VALIDACIONES BÁSICAS Y DATOS
    // ==========================================
    ParameterNull = 1001,
    ParameterEmpty = 1002,
    FormatInvalid = 1003,
    ValueOutOfRange = 1004,
    LengthExceeded = 1005,
    MissingRequiredData = 1006,
    InvalidCast = 1007,
    DataTypeMismatch = 1008, // No coincide con CodeTypeData
    InvalidLength = 1009,


    // ==========================================
    // 2000 - 2999: PROTOCOLO (TerbinProtocol)
    // ==========================================
    PacketMalformed = 2001,
    HeaderInvalid = 2002,
    PayloadTooLarge = 2003, // Supera MAX_PLD
    FragmentationError = 2004, // Error al procesar FRAGMENT_IN
    PacketOrderMismatch = 2005, // Ej: llegó FIRST_PACKET donde no tocaba
    FinalPacketMissing = 2006,
    ChecksumInvalid = 2007,
    ProtocolVersionUnknown = 2008,
    AlreadyExists = 2009,

    // ==========================================
    // 3000 - 3999: MEMORIA Y STREAMS (TerbinMemory)
    // ==========================================
    MemoryIdNotFound = 3001, // TryGetResult falló
    MemoryReleaseFailed = 3002,
    MemoryStoreFailed = 3003,
    MemoryAlreadyAllocated = 3004,
    BufferOverflow = 3005,
    StreamReadError = 3006,
    StreamWriteError = 3007,
    ReservedMemoryAccess = 3008, // Intento de usar RESERVE_MEMORY
    OrderMismatch = 3009,

    // ==========================================
    // 4000 - 4999: EJECUCIÓN (ExecutableDispatcher)
    // ==========================================
    ActionNotFound = 4001, // Handler no existe
    ActionAlreadyRegistered = 4002,
    ActionRegistrationFailed = 4003,
    HandlerExecutionFailed = 4004, // Excepción atrapada en DispatchAsync
    InvalidSignature = 4005, // Firma de método incorrecta para el Delegate

    // ==========================================
    // 5000 - 5999: ESTADO Y LÓGICA DE NEGOCIO
    // ==========================================
    ResourceNotFound = 5001,
    ResourceAlreadyExists = 5002,
    InvalidStateTransition = 5003,
    OperationCancelled = 5004,
    LimitExceeded = 5005,

    // ==========================================
    // 6000 - 6999: SEGURIDAD Y PERMISOS
    // ==========================================
    Unauthorized = 6001,
    ForbiddenAccess = 6002,
    SignatureInvalid = 6003,

    // ==========================================
    // 7000 - 7999: CONECTIVIDAD Y TIMEOUTS
    // ==========================================
    TimeoutExceeded = 7001, // Supera MAXIMUS_RESPONSE_TIME
    ConnectionLost = 7002,
    EndpointUnreachable = 7003,

    // ==========================================
    // 9000 - 9999: ERRORES FATALES DE WORKER
    // ==========================================
    InternalSystemError = 9000,
    OutOfMemory = 9001,
    NotImplementedYet = 9999
}

public enum AppError : int
{
    None = 0, // Operación exitosa

    // ==========================================
    // 100 - 199: VALIDACIÓN DE PARÁMETROS Y DATOS
    // ==========================================
    NullValue = 100, // Pasaste un null donde no debías
    EmptyValue = 101, // String vacío o colección sin elementos
    WhitespaceOnly = 102, // String con solo espacios
    ValueTooLow = 103, // Ej: Pasaste un negativo cuando se pedía positivo
    ValueTooHigh = 104, // Ej: Supera el máximo permitido
    LengthTooShort = 105, // Ej: Contraseña de 2 caracteres
    LengthTooLong = 106,
    InvalidFormat = 107, // Ej: Un email mal escrito o un regex que no cuadra
    InvalidType = 108, // El tipo de dato no es el esperado

    // ==========================================
    // 200 - 299: COLECCIONES Y BÚSQUEDAS
    // ==========================================
    ItemNotFound = 200, // No se encuentra en el diccionario/lista
    ItemAlreadyExists = 201, // Intentas añadir un ID que ya existe
    CollectionEmpty = 202, // Intentas procesar una lista que no tiene nada
    IndexOutOfRange = 203, // Índice de array incorrecto

    // ==========================================
    // 300 - 399: ESTADO Y LÓGICA DE NEGOCIO
    // ==========================================
    InvalidState = 300, // Ej: Intentas "Pausar" algo que ya está "Pausado"
    NotAllowed = 301, // Operación no permitida en este contexto
    DependencyMissing = 302, // Falta algo necesario para continuar
    LimitExceeded = 303, // Se superó una cuota o límite lógico
    Expired = 304, // Un token, sesión o dato ha caducado

    // ==========================================
    // 400 - 499: PERMISOS Y SEGURIDAD
    // ==========================================
    Unauthorized = 400, // No has iniciado sesión / No te conozco
    Forbidden = 401, // Te conozco, pero no tienes permisos para esto
    InvalidCredentials = 402, // Usuario o contraseña incorrectos

    // ==========================================
    // 500 - 599: INFRAESTRUCTURA Y EXTERNOS (Red, IO)
    // ==========================================
    Timeout = 500, // La operación tardó demasiado
    ConnectionFailed = 501, // Fallo de NamedPipes, Sockets, etc.
    ResourceLocked = 502, // Un archivo o memoria está en uso por otro hilo
    ReadError = 503, // Error al leer del disco o stream
    WriteError = 504, // Error al escribir en disco o stream

    // ==========================================
    // 900 - 999: ERRORES NO CONTROLADOS / CRÍTICOS
    // ==========================================
    NotImplemented = 900, // La función aún no está programada
    InternalError = 999  // Error genérico inesperado ("Ha pasado algo raro")
}