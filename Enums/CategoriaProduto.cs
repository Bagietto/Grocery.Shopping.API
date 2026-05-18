namespace Grocery.Shopping.API.Enums
{
    public enum CategoriaProduto
    {
        // --- Alimentos básicos / secos ---
        Graos = 1,                  // arroz, feijão, lentilha
        Massas = 2,                 // macarrão, miojo
        Farinhas = 3,               // farinha de trigo, mandioca
        Acucares = 4,               // açúcar refinado, mascavo
        Cafes = 5,                  // café moído, solúvel
        CereaisMatinais = 6,        // sucrilhos, granola
        OleosEGorduras = 7,         // óleo soja, azeite, margarina
        Enlatados = 8,              // milho, ervilha, atum
        Condimentos = 9,            // sal, pimenta, páprica, ervas
        TemperosMolhos = 10,        // ketchup, mostarda, shoyu
        Encurtidos = 11,            // azeitona, pepino
        Snacks = 12,                // biscoitos salgados, salgadinhos

        // --- Doces & Açucarados ---
        BiscoitosDoces = 20,
        Chocolates = 21,
        SobremesasProntas = 22,     // pudim, gelatina
        Confeitaria = 23,           // granulados, coberturas

        // --- Padaria ---
        Paes = 30,
        Bolos = 31,
        Tortas = 32,
        Salgados = 33,

        // --- Hortifruti ---
        Frutas = 40,
        Verduras = 41,
        Legumes = 42,
        ErvasFrescas = 43,

        // --- Frios / Laticínios ---
        Laticinios = 50,            // leite, manteiga
        Iogurtes = 51,
        Queijos = 52,
        Embutidos = 53,             // presunto, salame
        Ovos = 54,

        // --- Geladeira / Congelados ---
        Carnes = 60,
        Aves = 61,
        Peixes = 62,
        PratosCongelados = 63,
        LegumesCongelados = 64,

        // --- Bebidas ---
        BebidasNaoAlcoolicas = 70,  // suco, refrigerante, água
        Energéticos = 71,
        BebidasAlcoolicas = 72,     // cerveja, vinho
        CafesESolúveis = 73,
        Cha = 74,

        // --- Higiene pessoal ---
        Sabonetes = 80,
        Shampoos = 81,
        Condicionadores = 82,
        CremeDental = 83,
        EscovasDentais = 84,
        Desodorantes = 85,
        Barbear = 86,
        CuidadoFacial = 87,
        CuidadoCorporal = 88,
        HigieneIntima = 89,

        // --- Limpeza ---
        LavaRoupas = 100,
        Amaciantes = 101,
        LavaLoucas = 102,
        Desinfetantes = 103,
        Multiuso = 104,
        Detergentes = 105,
        Alcool = 106,
        Esponjas = 107,
        SacosLixo = 108,

        // --- Pets ---
        PetRacoes = 120,
        PetSnacks = 121,
        PetHigiene = 122,
        PetAcessorios = 123,

        // --- Bebê ---
        Fraldas = 130,
        LençosUmidecidos = 131,
        Papinhas = 132,
        LeitesInfantis = 133,

        // --- Descartáveis ---
        CoposDescartaveis = 140,
        PratosDescartaveis = 141,
        Talheres = 142,
        Guardanapos = 143,
        PapelToalha = 144,
        PapelHigienico = 145,

        // --- Miscelânea / Outros ---
        UtilidadesDomesticas = 160,
        Ferramentas = 161,
        Automotivo = 162,
        Outros = 999
    }

}
