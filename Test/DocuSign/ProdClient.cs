using DocuSign.eSign.Api;
using DocuSign.eSign.Client;

public class ProdClient
{
    public const string AccountId = "16219c5f-b3f5-4665-b446-c16689f33d8e";
    public static DocuSignClient GetClient()
    {
        string privateKey = """
            -----BEGIN RSA PRIVATE KEY-----
            MIIEowIBAAKCAQEAxDaM6tL56URyb8Fja4gsKk6obsfI7d8FSaUg7XEugvA5/FAC
            DHuVcAE5wU3Qv4lLW/7P0K2dKlIW8tBRKOHE/STTRUocnbvdvHt2nPNXQEfIZAY4
            6iyKkAvQB/Y74pyuUlEO49NKSgGQGbjEEN1PQ9V2FzWZGlOlam3ZbfTkqXFPm5/S
            wpOHvJrOiuNpdkfcBzLZMAZp6mi19Z7P//BaLm0Kr09YnVJxoet3Sfe2ipwfYEzM
            QfC3wDPIVWaLrOENsea9o6Zi8nc9p1S25+FjjnynBqQ8mk9TM90UJyegHjhdqRDu
            rWec+TA2miRGrY6xewnZgSz9+i/7E1AsptF+gwIDAQABAoIBAB4oaEZbgQ/qdlBZ
            AAfyGR+zkU3dpTNyeOSV3dbA864qcC8ouPfkMtbRSWg9pp0Z5BxSsOxZEvQDRAW0
            IRVQhi+GbnaS6o0P6AflThjXigyWO+Wr0ymjCPUU67edkBzHij+at9gqjJuNW/Go
            JXKWIW3CrHDqywpx2tXd5nJxFog4XlYmh9rQw4e2dBwo1rb6JnYFpLzxzO+b+Ras
            sCotYS//UKJmLvdSb9glANydFegdVxxBkfFINFChWC9x5+5f8qRZU4nvFOgn9exZ
            Yj8v8ZvzfeM206kf/Qb7bZoP/Z3WvFYNvRo7wb7mqBHvc6JpmEmXWoN2kFUSLGfq
            VRPgM2kCgYEA6ldZblXB74M6JeAlafWMo4dT1+znJK7UKjhXwxWzMpo5q+4vDm16
            naTu4sV7VJqd0iJinHLpxXaWlxG1qpSDvsg08B3kTXXgrPgCqO04RiQ2nIflK8KI
            80JDBa9IEkvstaMHzORusP3W0+do59AbNN8F8YQpoPwcXJ620oPezM8CgYEA1lkR
            QB/ZFUbSfqqq0DYRKUwZY1jc/hZTMACHqHr8Y6YubHXG31TjG53qOxF8mRtyIptL
            snFe/g8EVD12ShNCXlT4R67p8BiPZXJrbbNxxHEVBURm5/pFcXsBmwx05ckJJGhJ
            P6xVdNj0AggpgJ5xXAx7whmaOpCuDPSRS78jaA0CgYBcGXpnd8LhvER1MFPkAgKF
            HqGgIlZxv1hZQ42SeYvVHnH+FX1fAT4IiRLuA0lGZgIS3Tq+XBduaP4kX2xznyzs
            JUlQ+Z6JpwNnV43MEHdmccMY3/v8p9clK+ylZk9ACaD9fhaJu1mZrBnrbG55dPvM
            Wr6+PgASaiNHb4VQ1U0SVwKBgQCl6btYj05REcjNEv20vKT7+lOMerRUWKN7fG2F
            E+1YlMFKYr/VDrfcIJe1sQto172428v0C50juiv3qLtvCwlMSykDE3kwx1H3jGFr
            QILHM2C5+wBMf4RRGo1bnoC9fKb+71oDVzmugGAfUoINdJb6UQ9aZsbiniqbDJOK
            tiCJ4QKBgF0VzxFfPtPnytrYtiuWuD/BpnxfgnAlaLokiyd6HhqkuZgek9WgQ7/t
            29X8ivuMOkFH8uM9hJmuT2spFAwltNGYY+IgexDSMJ5zfBzIK4PQ4mF4epaJfaQ9
            kJbqkkrSVzKbAr5x6DYVlbN29R0fDIMfN2zdlt22fJ5M7vMHz8zo
            -----END RSA PRIVATE KEY-----
        """;

        var client = new DocuSignClient("https://eu.docusign.net/restapi");
        client.RequestJWTUserToken(
                        "d4e93ff3-6a63-4b8b-8895-ef7297aa0943",
                        "e2d9d960-1c89-4e55-90fa-26d20fc6850d",
                        "account.docusign.com",
                        System.Text.Encoding.Default.GetBytes(privateKey),
                        1);

        return client;
    }
}