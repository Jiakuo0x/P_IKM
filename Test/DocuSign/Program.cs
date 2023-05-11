using DocuSign.eSign.Api;
using DocuSign.eSign.Client;

string privateKey = """
    -----BEGIN RSA PRIVATE KEY-----
    MIIEowIBAAKCAQEAlt5btJZBSYpeTXjhuEmmevufe3ryq50xdjbpI97qnAySc6Kx
    t6x2lf5GgVssOXHlTRVi79Tr1o8vRKNjDPJOi8ET1p1aie+l5iHiYyZj2yyfD9fv
    71e+asA8vnDVtIiDObZ3D+bYLodDFrEb9knHZZmYCndDvKGwXy1OELVwdrJ638Zz
    2p4lqFHHJKHE6wOfV8K6BvD2+9JHdaeqQRg0uTmI4fqhQIpavFOfJjWmxgTvK4nB
    uztCl0HINXgRGOeH/jWnd/OuXvu8gx7fa0/+8yjmrZXwkAOqnVci8M3uJue3biUE
    MJRJpc0v5EdKz3pJcEAF5vL0Z5joH6lVIthzYQIDAQABAoIBAASxr3D4RqWNa68f
    Mzq2KlSXDqVbZ0CF5NuUpf73n4OydArea+29YBH9Qt+64MhHlG0oQxdLBl/himsP
    mfRCd8VeZ3BQHgu+2QHgWdH6IeilKnlmORdqOsj8gtQg4G1V3NHHYExhqaYTXFGE
    IJmycV5//5yK86CwKs1gGsoA5mcy/+pLYwownfUje5kVx0+/YqC8cFAJyJHREYai
    WtmvYSW450F2E0Fq5C9LQuv0tUh/7AWVqwmF+nPD4K6wsf/IfnuTkYPNRH3uf1gu
    dW1D1tZxGiWpVdF0jxD80JaOurV0IzpjAYifSmeJf/KlDyPFdyl8PDih0cPcC2nr
    CDRq4XcCgYEA3f+AR2pp8cYIfsFjEtqJuufw96tdaX9q4g4MaNQm8vKEqoK43K+B
    o4bA6r8pDTJSsgbv4YXuttzRQ17+sTAKB+WyaPKwwhe8Rky99Z1HNTFYCsBI28t5
    MWS35RDptxa2xXylbKoIEkClz9q0jnbmkiBf2nEzjl9t2g81iZHdt5cCgYEArfni
    mV1TyLLc1PcQsjL9MOQxF6gCTtnrA0Pw/hpeCcHt7o8y6eeauZJFJzlvTIdUUMPR
    unH171sVetX5iG0HJeuJK8r6pavsmsyOSNCTGfIIbUz+gTPgq581K9mGLFGVAdjP
    1ovBOx7PYdO9BXwrJRuZTGg/FCqRA4xY9pWmy8cCgYB7dMGD9bvhRr4mr6lHLN13
    YdFyCoyyRLfN6v4ftgvLA++fW38uyzOPGzth0Nkli5zNgGoawv7UFs0RaFy/cPXD
    GowzLPP7nHOJrNffJY4aGMzbfb+G7AsD2v0hmFxBA5K1FPJyEcTXUbhkdT4AFEN5
    dCOaOWXwgUV4BQlC7imdFQKBgHpgIn+EgVHUVrfKzki6yxRf/xRHzs/OQ5x5ZwQm
    Ye11Jzs+KS8VBeXwuIn9wYdQTgO9qkH+tWLXbAWKi8rl/jgzNLrEPYjZpUXCC3e2
    lzKR6FGR7hfN+QRfqdQdX16/SBQTgSbGCXbfljqW6Qf5rpOclTmEvpId2wFm8JEK
    9VezAoGBAIzTYL6GO+rfu1+AP6ozmZkAwSSVtMPCFGpfpuLQ3aoJPOgIg6mlMADC
    hqEK5BTIx4N4THCrVfE3VhI6kRI9/UWZJlnsOTAx/9dt/hO2G1xpJBN1vP4FaXNT
    1fdGfZIXh+KC3K+teRuSFrLjSmkmnBtHFXtFG9N97wmBNyNUSJMp
    -----END RSA PRIVATE KEY-----
""";

var client = new DocuSignClient("https://demo.docusign.net/restapi");
client.RequestJWTUserToken(
                "f90b1c10-0743-4870-a631-666682d11320",
               "dd77674b-38d3-4df8-9d71-ed9e1e4613a1",
                "account-d.docusign.com",
                System.Text.Encoding.Default.GetBytes(privateKey),
                 1);

EnvelopesApi envelopesApi = new(client);
var templates = envelopesApi.ListTemplates("3363b373-d55d-4487-8828-13154df36834", "f039c311-fab8-41e0-b80b-3580604af315");
Console.ReadLine();