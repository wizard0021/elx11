// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 PES5 Indie Team <pes5indie@gmail.com>

using Microsoft.AspNetCore.Http;

namespace OpenEleven.Server.Web.Localization;

public sealed class WebLocalizer(IHttpContextAccessor http) : IWebLocalizer
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Nav_Home"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Home",
            ["tr"] = "Ana Sayfa",
            ["de"] = "Startseite",
            ["fr"] = "Accueil",
            ["es"] = "Inicio",
            ["it"] = "Home",
            ["pt"] = "Início",
            ["nl"] = "Home",
        },
        ["Nav_Rankings"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Rankings",
            ["tr"] = "Sıralama",
            ["de"] = "Rangliste",
            ["fr"] = "Classements",
            ["es"] = "Clasificación",
            ["it"] = "Classifiche",
            ["pt"] = "Classificação",
            ["nl"] = "Ranglijst",
        },
        ["Nav_Players"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Players",
            ["tr"] = "Oyuncular",
            ["de"] = "Spieler",
            ["fr"] = "Joueurs",
            ["es"] = "Jugadores",
            ["it"] = "Giocatori",
            ["pt"] = "Jogadores",
            ["nl"] = "Spelers",
        },
        ["Nav_Matches"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Matches",
            ["tr"] = "Maçlar",
            ["de"] = "Spiele",
            ["fr"] = "Matchs",
            ["es"] = "Partidos",
            ["it"] = "Partite",
            ["pt"] = "Partidas",
            ["nl"] = "Wedstrijden",
        },
        ["Nav_HardwareSurvey"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Hardware Survey",
            ["tr"] = "Donanım Anketi",
            ["de"] = "Hardware-Umfrage",
            ["fr"] = "Enquête Matériel",
            ["es"] = "Encuesta de Hardware",
            ["it"] = "Sondaggio Hardware",
            ["pt"] = "Pesquisa de Hardware",
            ["nl"] = "Hardware-enquête",
        },
        ["Nav_Roadmap"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Roadmap",
            ["tr"] = "Yol Haritası",
            ["de"] = "Roadmap",
            ["fr"] = "Feuille de route",
            ["es"] = "Hoja de ruta",
            ["it"] = "Tabella di marcia",
            ["pt"] = "Roteiro",
            ["nl"] = "Roadmap",
        },
        ["Nav_Register"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Register",
            ["tr"] = "Kayıt Ol",
            ["de"] = "Registrieren",
            ["fr"] = "S'inscrire",
            ["es"] = "Registrarse",
            ["it"] = "Registrati",
            ["pt"] = "Registrar",
            ["nl"] = "Registreren",
        },
        ["Nav_Site"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Site",
            ["tr"] = "Siteye Dön",
            ["de"] = "Zur Seite",
            ["fr"] = "Site",
            ["es"] = "Sitio",
            ["it"] = "Sito",
            ["pt"] = "Site",
            ["nl"] = "Site",
        },
        ["Nav_SignOut"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Sign out",
            ["tr"] = "Çıkış Yap",
            ["de"] = "Abmelden",
            ["fr"] = "Déconnexion",
            ["es"] = "Cerrar sesión",
            ["it"] = "Disconnetti",
            ["pt"] = "Sair",
            ["nl"] = "Uitloggen",
        },
        ["Nav_Menu"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Menu",
            ["tr"] = "Menü",
            ["de"] = "Menü",
            ["fr"] = "Menu",
            ["es"] = "Menú",
            ["it"] = "Menu",
            ["pt"] = "Menu",
            ["nl"] = "Menu",
        },
        ["Footer_FanServer"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "a fan-run community server for",
            ["tr"] = "için taraftar yapımı topluluk sunucusu",
            ["de"] = "ein Fan-betriebener Community-Server für",
            ["fr"] = "un serveur communautaire de fans pour",
            ["es"] = "un servidor comunitario hecho por fans para",
            ["it"] = "un server comunitario gestito dai fan per",
            ["pt"] = "um servidor comunitário feito por fãs para",
            ["nl"] = "een door fans gerunde communityserver voor",
        },
        ["Footer_PoweredBy"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Powered by OpenEleven",
            ["tr"] = "OpenEleven Altyapısıyla",
            ["de"] = "Unterstützt von OpenEleven",
            ["fr"] = "Propulsé par OpenEleven",
            ["es"] = "Desarrollado con OpenEleven",
            ["it"] = "Basato su OpenEleven",
            ["pt"] = "Desenvolvido com OpenEleven",
            ["nl"] = "Aangedreven door OpenEleven",
        },
        ["Footer_BackToTop"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Back to top",
            ["tr"] = "Başa dön",
            ["de"] = "Nach oben",
            ["fr"] = "Haut de page",
            ["es"] = "Volver arriba",
            ["it"] = "Torna su",
            ["pt"] = "Voltar ao topo",
            ["nl"] = "Terug naar boven",
        },
        ["Footer_CreateAccount"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Create account",
            ["tr"] = "Hesap oluştur",
            ["de"] = "Konto erstellen",
            ["fr"] = "Créer un compte",
            ["es"] = "Crear cuenta",
            ["it"] = "Crea account",
            ["pt"] = "Criar conta",
            ["nl"] = "Account aanmaken",
        },
        ["Hero_Tagline"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "PES 2011 online multiplayer nostalgia is back. Free, ad-free, fan-powered.",
            ["tr"] = "PES 2011 çevrim içi çok oyunculu nostaljisi geri döndü. Ücretsiz, reklamsız, taraftar destekli.",
            ["de"] = "PES 2011 Online-Multiplayer-Nostalgie ist zurück. Kostenlos, werbefrei, von Fans betrieben.",
            ["fr"] = "La nostalgie du multijoueur en ligne de PES 2011 est de retour. Gratuit, sans pub, créé par des fans.",
            ["es"] = "La nostalgia del multijugador online de PES 2011 está de vuelta. Gratis, sin anuncios, impulsado por fans.",
            ["it"] = "La nostalgia del multiplayer online di PES 2011 è tornata. Gratuito, senza pubblicità, creato dai fan.",
            ["pt"] = "A nostalgia do multiplayer online do PES 2011 está de volta. Gratuito, sem anúncios, movido por fãs.",
            ["nl"] = "PES 2011 online multiplayer nostalgie is terug. Gratis, reclamevrij, mogelijk gemaakt door fans.",
        },
        ["Hero_CreateAccount"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Create an account",
            ["tr"] = "Hesap oluştur",
            ["de"] = "Konto erstellen",
            ["fr"] = "Créer un compte",
            ["es"] = "Crear una cuenta",
            ["it"] = "Crea un account",
            ["pt"] = "Criar uma conta",
            ["nl"] = "Maak een account aan",
        },
        ["Hero_DownloadLauncher"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Download Launcher",
            ["tr"] = "Başlatıcıyı İndir",
            ["de"] = "Launcher herunterladen",
            ["fr"] = "Télécharger le lanceur",
            ["es"] = "Descargar Launcher",
            ["it"] = "Scarica Launcher",
            ["pt"] = "Baixar Launcher",
            ["nl"] = "Download Launcher",
        },
        ["Hero_JoinDiscord"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Join Discord",
            ["tr"] = "Discord'a Katıl",
            ["de"] = "Discord beitreten",
            ["fr"] = "Rejoindre Discord",
            ["es"] = "Unirse a Discord",
            ["it"] = "Entra su Discord",
            ["pt"] = "Entrar no Discord",
            ["nl"] = "Word lid van Discord",
        },
        ["Tile_OnlineNow"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Online now",
            ["tr"] = "Şu an çevrim içi",
            ["de"] = "Jetzt online",
            ["fr"] = "En ligne",
            ["es"] = "En línea ahora",
            ["it"] = "Ora online",
            ["pt"] = "Online agora",
            ["nl"] = "Nu online",
        },
        ["Tile_PlayingNow"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Playing now",
            ["tr"] = "Şu an oyunda",
            ["de"] = "Im Spiel",
            ["fr"] = "En match",
            ["es"] = "Jugando ahora",
            ["it"] = "In partita",
            ["pt"] = "Jogando agora",
            ["nl"] = "Nu aan het spelen",
        },
        ["Tile_Players"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Players",
            ["tr"] = "Oyuncular",
            ["de"] = "Spieler",
            ["fr"] = "Joueurs",
            ["es"] = "Jugadores",
            ["it"] = "Giocatori",
            ["pt"] = "Jogadores",
            ["nl"] = "Spelers",
        },
        ["Tile_Matches"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Matches",
            ["tr"] = "Maçlar",
            ["de"] = "Spiele",
            ["fr"] = "Matchs",
            ["es"] = "Partidos",
            ["it"] = "Partite",
            ["pt"] = "Partidas",
            ["nl"] = "Wedstrijden",
        },
        ["Panel_LiveMatches"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Live matches",
            ["tr"] = "Canlı maçlar",
            ["de"] = "Live-Spiele",
            ["fr"] = "Matchs en direct",
            ["es"] = "Partidos en vivo",
            ["it"] = "Partite in diretta",
            ["pt"] = "Partidas ao vivo",
            ["nl"] = "Live wedstrijden",
        },
        ["Panel_NobodyPlaying"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Nobody is playing right now.",
            ["tr"] = "Şu anda oynanan maç yok.",
            ["de"] = "Im Moment spielt niemand.",
            ["fr"] = "Personne ne joue actuellement.",
            ["es"] = "Nadie está jugando en este momento.",
            ["it"] = "Nessuno sta giocando al momento.",
            ["pt"] = "Ninguém está jogando no momento.",
            ["nl"] = "Er speelt momenteel niemand.",
        },
        ["Panel_TopPlayers"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Top players",
            ["tr"] = "En iyi oyuncular",
            ["de"] = "Beste Spieler",
            ["fr"] = "Meilleurs joueurs",
            ["es"] = "Mejores jugadores",
            ["it"] = "Migliori giocatori",
            ["pt"] = "Melhores jogadores",
            ["nl"] = "Beste spelers",
        },
        ["Panel_NoRanked"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "No ranked players yet.",
            ["tr"] = "Henüz sıralamaya giren oyuncu yok.",
            ["de"] = "Noch keine Ranglistenspieler.",
            ["fr"] = "Pas encore de joueurs classés.",
            ["es"] = "Aún no hay jugadores clasificados.",
            ["it"] = "Nessun giocatore classificato.",
            ["pt"] = "Nenhum jogador classificado ainda.",
            ["nl"] = "Nog geen gerankte spelers.",
        },
        ["Panel_RecentMatches"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Recent matches",
            ["tr"] = "Son maçlar",
            ["de"] = "Letzte Spiele",
            ["fr"] = "Matchs récents",
            ["es"] = "Partidos recientes",
            ["it"] = "Partite recenti",
            ["pt"] = "Partidas recentes",
            ["nl"] = "Recente wedstrijden",
        },
        ["Panel_NoMatches"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "No matches played yet.",
            ["tr"] = "Henüz maç yapılmadı.",
            ["de"] = "Noch keine Spiele gespielt.",
            ["fr"] = "Aucun match joué pour le moment.",
            ["es"] = "Aún no se han jugado partidos.",
            ["it"] = "Nessuna partita giocata.",
            ["pt"] = "Nenhuma partida jogada ainda.",
            ["nl"] = "Nog geen wedstrijden gespeeld.",
        },
        ["Panel_CommunityNews"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Community News & Updates",
            ["tr"] = "Topluluk Duyuruları & Güncellemeler",
            ["de"] = "Community-Neuigkeiten & Updates",
            ["fr"] = "Nouvelles & Mises à jour de la communauté",
            ["es"] = "Noticias y Actualizaciones de la Comunidad",
            ["it"] = "Notizie & Aggiornamenti della Community",
            ["pt"] = "Notícias e Atualizações da Comunidade",
            ["nl"] = "Community-nieuws & Updates",
        },
        ["Panel_NoNews"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "No news published yet.",
            ["tr"] = "Henüz duyuru paylaşılmadı.",
            ["de"] = "Noch keine Neuigkeiten veröffentlicht.",
            ["fr"] = "Aucune nouvelle publiée pour le moment.",
            ["es"] = "Aún no se han publicado noticias.",
            ["it"] = "Nessuna notizia pubblicata.",
            ["pt"] = "Nenhuma notícia publicada ainda.",
            ["nl"] = "Nog geen nieuws gepubliceerd.",
        },
        ["Link_AllRankings"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Rankings ›",
            ["tr"] = "Sıralama ›",
            ["de"] = "Rangliste ›",
            ["fr"] = "Classements ›",
            ["es"] = "Clasificación ›",
            ["it"] = "Classifiche ›",
            ["pt"] = "Classificação ›",
            ["nl"] = "Ranglijst ›",
        },
        ["Link_AllMatches"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "All matches ›",
            ["tr"] = "Tüm maçlar ›",
            ["de"] = "Alle Spiele ›",
            ["fr"] = "Tous les matchs ›",
            ["es"] = "Todos los partidos ›",
            ["it"] = "Tutte le partite ›",
            ["pt"] = "Todas as partidas ›",
            ["nl"] = "Alle wedstrijden ›",
        },
        ["Connect_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "How to Connect & Play",
            ["tr"] = "Nasıl Bağlanılır ve Oynanır?",
            ["de"] = "Wie man beitritt und spielt",
            ["fr"] = "Comment se connecter et jouer",
            ["es"] = "Cómo conectarse y jugar",
            ["it"] = "Come connettersi e giocare",
            ["pt"] = "Como conectar e jogar",
            ["nl"] = "Hoe te verbinden en spelen",
        },
        ["Connect_Subtitle"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Follow the steps below to join the ELX11 PES 2011 server.",
            ["tr"] = "ELX11 PES 2011 sunucusuna katılmak için aşağıdaki adımları izleyin.",
            ["de"] = "Folgen Sie den Schritten unten, um dem ELX11 PES 2011 Server beizutreten.",
            ["fr"] = "Suivez les étapes ci-dessous pour rejoindre le serveur ELX11 PES 2011.",
            ["es"] = "Sigue los pasos a continuación para unirte al servidor ELX11 PES 2011.",
            ["it"] = "Segui i passaggi seguenti per unirti al server ELX11 PES 2011.",
            ["pt"] = "Siga os passos abaixo para entrar no servidor ELX11 PES 2011.",
            ["nl"] = "Volg de onderstaande stappen om lid te worden van de ELX11 PES 2011 server.",
        },
        ["Connect_Opt1Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Option 1 · Automated Launcher (Recommended)",
            ["tr"] = "Seçenek 1 · Otomatik Başlatıcı (Önerilen)",
            ["de"] = "Option 1 · Automatischer Launcher (Empfohlen)",
            ["fr"] = "Option 1 · Lanceur automatique (Recommandé)",
            ["es"] = "Opción 1 · Launcher Automático (Recomendado)",
            ["it"] = "Opzione 1 · Launcher Automatico (Consigliato)",
            ["pt"] = "Opção 1 · Launcher Automático (Recomendado)",
            ["nl"] = "Optie 1 · Automatische Launcher (Aanbevolen)",
        },
        ["Connect_Opt2Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Option 2 · Manual Hosts / DNS Setup",
            ["tr"] = "Seçenek 2 · Manuel Hosts / DNS Kurulumu",
            ["de"] = "Option 2 · Manuelle Hosts / DNS-Einrichtung",
            ["fr"] = "Option 2 · Configuration manuelle Hosts / DNS",
            ["es"] = "Opción 2 · Configuración manual de Hosts / DNS",
            ["it"] = "Opzione 2 · Configurazione manuale Hosts / DNS",
            ["pt"] = "Opção 2 · Configuração manual de Hosts / DNS",
            ["nl"] = "Optie 2 · Handmatige Hosts / DNS-instelling",
        },
        ["Roadmap_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "ELX11 Project Roadmap",
            ["tr"] = "ELX11 Proje Yol Haritası",
            ["de"] = "ELX11 Projekt-Roadmap",
            ["fr"] = "Feuille de route du projet ELX11",
            ["es"] = "Hoja de ruta del proyecto ELX11",
            ["it"] = "Tabella di marcia del progetto ELX11",
            ["pt"] = "Roteiro do Projeto ELX11",
            ["nl"] = "ELX11 Project-Roadmap",
        },
        ["Roadmap_Desc"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Evolution Legacy X 11 development milestones, open beta and future vision.",
            ["tr"] = "Evolution Legacy X 11 geliştirme aşamaları, açık beta ve gelecek vizyonu.",
            ["de"] = "Evolution Legacy X 11 Entwicklungsmeilensteine, Open Beta und Zukunftsvision.",
            ["fr"] = "Jalons de développement d'Evolution Legacy X 11, bêta ouverte et vision future.",
            ["es"] = "Hitos de desarrollo de Evolution Legacy X 11, beta abierta y visión de futuro.",
            ["it"] = "Traguardi di sviluppo di Evolution Legacy X 11, open beta e visione futura.",
            ["pt"] = "Marcos de desenvolvimento do Evolution Legacy X 11, beta aberta e visão futura.",
            ["nl"] = "Evolution Legacy X 11 ontwikkelingsmijlpalen, open bèta en toekomstvisie.",
        },
        ["Roadmap_OpenBetaActive"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "● OPEN BETA ACTIVE",
            ["tr"] = "● AÇIK BETA AKTİF",
            ["de"] = "● OPEN BETA AKTIV",
            ["fr"] = "● BÊTA OUVERTE ACTIVE",
            ["es"] = "● BETA ABIERTA ACTIVA",
            ["it"] = "● OPEN BETA ATTIVA",
            ["pt"] = "● BETA ABERTA ATIVA",
            ["nl"] = "● OPEN BÈTA ACTIEF",
        },
        ["Roadmap_Phase1Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Core Infrastructure",
            ["tr"] = "Temel Altyapı",
            ["de"] = "Kerninfrastruktur",
            ["fr"] = "Infrastructure de base",
            ["es"] = "Infraestructura Central",
            ["it"] = "Infrastruttura Principale",
            ["pt"] = "Infraestrutura Principal",
            ["nl"] = "Kerninfrastructuur",
        },
        ["Roadmap_Phase1Desc"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Reverse-engineered Konami binary protocol and server backbone built from scratch in .NET 9.",
            ["tr"] = "Tersine mühendislikle çözülen Konami ikili protokolü ve .NET 9 ile sıfırdan inşa edilen sunucu omurgası.",
            ["de"] = "Reverse-Engineered Konami-Binärprotokoll und von Grund auf in .NET 9 gebautes Server-Backbone.",
            ["fr"] = "Protocole binaire Konami rétro-conçu et dorsale serveur construite de zéro en .NET 9.",
            ["es"] = "Protocolo binario de Konami sometido a ingeniería inversa y servidor creado desde cero en .NET 9.",
            ["it"] = "Protocollo binario Konami decodificato e backbone del server creato da zero in .NET 9.",
            ["pt"] = "Protocolo binário da Konami feito por engenharia reversa e servidor construído do zero em .NET 9.",
            ["nl"] = "Reverse-engineered Konami binair protocol en serverbackbone van de grond af opgebouwd in .NET 9.",
        },
        ["Roadmap_Phase2Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Cross-Platform & Beta",
            ["tr"] = "Platformlar Arası & Beta",
            ["de"] = "Plattformübergreifend & Beta",
            ["fr"] = "Multiplateforme & Bêta",
            ["es"] = "Multiplataforma y Beta",
            ["it"] = "Cross-Platform & Beta",
            ["pt"] = "Multiplataforma & Beta",
            ["nl"] = "Cross-platform & Bèta",
        },
        ["Roadmap_Phase2Desc"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Open beta is now live, bringing PC and PlayStation 3 players together on the same server!",
            ["tr"] = "Açık beta yayında; PC ve PlayStation 3 oyuncularını aynı sunucuda buluşturuyor!",
            ["de"] = "Open Beta ist live und bringt PC- und PlayStation 3-Spieler auf demselben Server zusammen!",
            ["fr"] = "La bêta ouverte est lancée, réunissant joueurs PC et PlayStation 3 sur le même serveur !",
            ["es"] = "¡La beta abierta ya está disponible, uniendo a jugadores de PC y PlayStation 3 en el mismo servidor!",
            ["it"] = "L'open beta è attiva, unendo giocatori PC e PlayStation 3 sullo stesso server!",
            ["pt"] = "A beta aberta está no ar, reunindo jogadores de PC e PlayStation 3 no mesmo servidor!",
            ["nl"] = "Open bèta is nu live en brengt pc- en PlayStation 3-spelers samen op dezelfde server!",
        },
        ["Roadmap_Phase3Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Matchmaking & Cups",
            ["tr"] = "Eşleştirme & Kupalar",
            ["de"] = "Matchmaking & Pokale",
            ["fr"] = "Matchs & Coupes",
            ["es"] = "Emparejamiento y Copas",
            ["it"] = "Matchmaking & Coppe",
            ["pt"] = "Matchmaking & Copas",
            ["nl"] = "Matchmaking & Bekers",
        },
        ["Roadmap_Phase3Desc"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Automated skill-based matchmaking algorithms and official tournament bracket engine.",
            ["tr"] = "Yetenek puanı tabanlı otomatik eşleştirme algoritmaları ve turnuva fikstür motoru.",
            ["de"] = "Automatische fähigkeitsbasierte Matchmaking-Algorithmen und Turnier-Engine.",
            ["fr"] = "Algorithmes de matchmaking automatique basés sur le niveau et moteur de tournois.",
            ["es"] = "Algoritmos de emparejamiento automático por nivel y motor de torneos oficiales.",
            ["it"] = "Algoritmi di matchmaking automatico basati sulle abilità e motore per tornei ufficiali.",
            ["pt"] = "Algoritmos de matchmaking automático por habilidade e motor de torneios oficiais.",
            ["nl"] = "Automatische op vaardigheden gebaseerde matchmaking-algoritmen en toernooimotor.",
        },
        ["Roadmap_Phase4Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Master League Online",
            ["tr"] = "Master League Online",
            ["de"] = "Master League Online",
            ["fr"] = "Master League Online",
            ["es"] = "Master League Online",
            ["it"] = "Master League Online",
            ["pt"] = "Master League Online",
            ["nl"] = "Master League Online",
        },
        ["Roadmap_Phase4Desc"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "The heartbeat of PES 2011: Full revival of the legendary Master League Online ecosystem.",
            ["tr"] = "PES 2011'in kalbi: Efsanevi Master League Online ekosisteminin eksiksiz geri dönüşü.",
            ["de"] = "Das Herzstück von PES 2011: Vollständiges Wiederaufleben des legendären Master League Online-Ökosystems.",
            ["fr"] = "Le cœur battant de PES 2011 : renaissance complète de la légendaire Master League Online.",
            ["es"] = "El corazón de PES 2011: Renacimiento total del legendario ecosistema Master League Online.",
            ["it"] = "Il cuore pulsante di PES 2011: Rinascita completa della leggendaria Master League Online.",
            ["pt"] = "O coração do PES 2011: Renascimento completo do lendário ecossistema Master League Online.",
            ["nl"] = "Het hart van PES 2011: Volledige herleving van het legendarische Master League Online-ecosysteem.",
        },
        ["Rankings_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Rankings",
            ["tr"] = "Sıralama",
            ["de"] = "Rangliste",
            ["fr"] = "Classements",
            ["es"] = "Clasificación",
            ["it"] = "Classifiche",
            ["pt"] = "Classificação",
            ["nl"] = "Ranglijst",
        },
        ["Tabs_AllTime"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "All time",
            ["tr"] = "Tüm zamanlar",
            ["de"] = "Gesamt",
            ["fr"] = "Tous les temps",
            ["es"] = "Todos los tiempos",
            ["it"] = "Tutti i tempi",
            ["pt"] = "Todos os tempos",
            ["nl"] = "Altijd",
        },
        ["Tabs_ThisWeek"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "This week",
            ["tr"] = "Bu hafta",
            ["de"] = "Diese Woche",
            ["fr"] = "Cette semaine",
            ["es"] = "Esta semana",
            ["it"] = "Questa settimana",
            ["pt"] = "Esta semana",
            ["nl"] = "Deze week",
        },
        ["Tabs_AllDivisions"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "All divisions",
            ["tr"] = "Tüm kümeler",
            ["de"] = "Alle Divisionen",
            ["fr"] = "Toutes les divisions",
            ["es"] = "Todas las divisiones",
            ["it"] = "Tutte le divisioni",
            ["pt"] = "Todas as divisões",
            ["nl"] = "Alle divisies",
        },
        ["Rankings_EmptyWeek"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Nobody has played this week.",
            ["tr"] = "Bu hafta henüz kimse maç yapmadı.",
            ["de"] = "Diese Woche hat noch niemand gespielt.",
            ["fr"] = "Personne n'a joué cette semaine.",
            ["es"] = "Nadie ha jugado esta semana.",
            ["it"] = "Nessuno ha giocato questa settimana.",
            ["pt"] = "Ninguém jogou esta semana.",
            ["nl"] = "Niemand heeft deze week gespeeld.",
        },
        ["Rankings_EmptyAll"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Nobody has played yet.",
            ["tr"] = "Henüz kimse maç yapmadı.",
            ["de"] = "Es hat noch niemand gespielt.",
            ["fr"] = "Personne n'a encore joué.",
            ["es"] = "Nadie ha jugado aún.",
            ["it"] = "Nessuno ha ancora giocato.",
            ["pt"] = "Ninguém jogou ainda.",
            ["nl"] = "Niemand heeft nog gespeeld.",
        },
        ["Players_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Players",
            ["tr"] = "Oyuncular",
            ["de"] = "Spieler",
            ["fr"] = "Joueurs",
            ["es"] = "Jugadores",
            ["it"] = "Giocatori",
            ["pt"] = "Jogadores",
            ["nl"] = "Spelers",
        },
        ["Search_Placeholder"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Search by name",
            ["tr"] = "İsme göre ara",
            ["de"] = "Nach Namen suchen",
            ["fr"] = "Rechercher par nom",
            ["es"] = "Buscar por nombre",
            ["it"] = "Cerca per nome",
            ["pt"] = "Buscar por nome",
            ["nl"] = "Zoeken op naam",
        },
        ["Search_Button"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Search",
            ["tr"] = "Ara",
            ["de"] = "Suchen",
            ["fr"] = "Rechercher",
            ["es"] = "Buscar",
            ["it"] = "Cerca",
            ["pt"] = "Buscar",
            ["nl"] = "Zoeken",
        },
        ["Players_NoActive"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "No active in-game players yet.",
            ["tr"] = "Henüz aktif oyuncu bulunmuyor.",
            ["de"] = "Noch keine aktiven Spieler im Spiel.",
            ["fr"] = "Pas encore de joueurs actifs en jeu.",
            ["es"] = "Aún no hay jugadores activos en el juego.",
            ["it"] = "Nessun giocatore attivo nel gioco.",
            ["pt"] = "Nenhum jogador ativo no jogo ainda.",
            ["nl"] = "Nog geen actieve spelers in het spel.",
        },
        ["Players_NoMatch"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "No player matches that name.",
            ["tr"] = "Bu isimle eşleşen oyuncu bulunamadı.",
            ["de"] = "Kein Spieler passt zu diesem Namen.",
            ["fr"] = "Aucun joueur ne correspond à ce nom.",
            ["es"] = "Ningún jugador coincide con ese nombre.",
            ["it"] = "Nessun giocatore corrisponde a questo nome.",
            ["pt"] = "Nenhum jogador corresponde a esse nome.",
            ["nl"] = "Geen speler komt overeen met die naam.",
        },
        ["Players_RegisteredMembers"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Registered Members",
            ["tr"] = "Kayıtlı Üyeler",
            ["de"] = "Registrierte Mitglieder",
            ["fr"] = "Membres enregistrés",
            ["es"] = "Miembros Registrados",
            ["it"] = "Membri Registrati",
            ["pt"] = "Membros Registrados",
            ["nl"] = "Geregistreerde Leden",
        },
        ["Players_ReadyToPlay"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Registered · Ready to play",
            ["tr"] = "Kayıtlı · Oynamaya Hazır",
            ["de"] = "Registriert · Spielbereit",
            ["fr"] = "Enregistré · Prêt à jouer",
            ["es"] = "Registrado · Listo para jugar",
            ["it"] = "Registrato · Pronto a giocare",
            ["pt"] = "Registrado · Pronto para jogar",
            ["nl"] = "Geregistreerd · Klaar om te spelen",
        },
        ["Players_ViewProfile"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "View Profile ›",
            ["tr"] = "Profili Gör ›",
            ["de"] = "Profil ansehen ›",
            ["fr"] = "Voir le profil ›",
            ["es"] = "Ver Perfil ›",
            ["it"] = "Visualizza Profilo ›",
            ["pt"] = "Ver Perfil ›",
            ["nl"] = "Profiel bekijken ›",
        },
        ["Matches_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Matches",
            ["tr"] = "Maçlar",
            ["de"] = "Spiele",
            ["fr"] = "Matchs",
            ["es"] = "Partidos",
            ["it"] = "Partite",
            ["pt"] = "Partidas",
            ["nl"] = "Wedstrijden",
        },
        ["Col_Date"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Date (UTC)",
            ["tr"] = "Tarih (UTC)",
            ["de"] = "Datum (UTC)",
            ["fr"] = "Date (UTC)",
            ["es"] = "Fecha (UTC)",
            ["it"] = "Data (UTC)",
            ["pt"] = "Data (UTC)",
            ["nl"] = "Datum (UTC)",
        },
        ["Col_Home"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Home",
            ["tr"] = "Ev Sahibi",
            ["de"] = "Heim",
            ["fr"] = "Domicile",
            ["es"] = "Local",
            ["it"] = "Casa",
            ["pt"] = "Casa",
            ["nl"] = "Thuis",
        },
        ["Col_Score"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Score",
            ["tr"] = "Skor",
            ["de"] = "Ergebnis",
            ["fr"] = "Score",
            ["es"] = "Resultado",
            ["it"] = "Risultato",
            ["pt"] = "Placar",
            ["nl"] = "Score",
        },
        ["Col_Away"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Away",
            ["tr"] = "Deplasman",
            ["de"] = "Auswärts",
            ["fr"] = "Extérieur",
            ["es"] = "Visitante",
            ["it"] = "Trasferta",
            ["pt"] = "Visitante",
            ["nl"] = "Uit",
        },
        ["Tag_Ranked"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Ranked",
            ["tr"] = "Dereceli",
            ["de"] = "Rangliste",
            ["fr"] = "Classé",
            ["es"] = "Clasificatorio",
            ["it"] = "Classificata",
            ["pt"] = "Ranqueada",
            ["nl"] = "Gerankt",
        },
        ["Tag_ExtraTime"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "ET",
            ["tr"] = "Uzatma",
            ["de"] = "Verl.",
            ["fr"] = "Prol.",
            ["es"] = "Prórroga",
            ["it"] = "Supp.",
            ["pt"] = "Prorr.",
            ["nl"] = "Verl.",
        },
        ["Tag_Penalties"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Pens",
            ["tr"] = "Penaltılar",
            ["de"] = "Elfmeter",
            ["fr"] = "Tirs au but",
            ["es"] = "Penaltis",
            ["it"] = "Rigori",
            ["pt"] = "Pênaltis",
            ["nl"] = "Pen.",
        },
        ["Tag_Disconnect"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Disconnect",
            ["tr"] = "Bağlantı Koptu",
            ["de"] = "Verbindungsabbruch",
            ["fr"] = "Déconnexion",
            ["es"] = "Desconexión",
            ["it"] = "Disconnesso",
            ["pt"] = "Desconectado",
            ["nl"] = "Verbroken",
        },
        ["Col_Rank"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Rank",
            ["tr"] = "Sıra",
            ["de"] = "Rang",
            ["fr"] = "Rang",
            ["es"] = "Posición",
            ["it"] = "Posizione",
            ["pt"] = "Posição",
            ["nl"] = "Rang",
        },
        ["Col_Player"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Player",
            ["tr"] = "Oyuncu",
            ["de"] = "Spieler",
            ["fr"] = "Joueur",
            ["es"] = "Jugador",
            ["it"] = "Giocatore",
            ["pt"] = "Jogador",
            ["nl"] = "Speler",
        },
        ["Col_Division"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Div",
            ["tr"] = "Küme",
            ["de"] = "Liga",
            ["fr"] = "Div",
            ["es"] = "Div",
            ["it"] = "Div",
            ["pt"] = "Div",
            ["nl"] = "Div",
        },
        ["Col_Rating"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Rating",
            ["tr"] = "Puan",
            ["de"] = "Wertung",
            ["fr"] = "Note",
            ["es"] = "Puntuación",
            ["it"] = "Punteggio",
            ["pt"] = "Classificação",
            ["nl"] = "Rating",
        },
        ["Col_Points"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Pts",
            ["tr"] = "Puan",
            ["de"] = "Pkt",
            ["fr"] = "Pts",
            ["es"] = "Pts",
            ["it"] = "Pti",
            ["pt"] = "Pts",
            ["nl"] = "Ptn",
        },
        ["Col_MatchesShort"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "P",
            ["tr"] = "O",
            ["de"] = "Sp",
            ["fr"] = "J",
            ["es"] = "PJ",
            ["it"] = "P",
            ["pt"] = "J",
            ["nl"] = "G",
        },
        ["Col_WinShort"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "W",
            ["tr"] = "G",
            ["de"] = "S",
            ["fr"] = "V",
            ["es"] = "G",
            ["it"] = "V",
            ["pt"] = "V",
            ["nl"] = "W",
        },
        ["Col_DrawShort"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "D",
            ["tr"] = "B",
            ["de"] = "U",
            ["fr"] = "N",
            ["es"] = "E",
            ["it"] = "P",
            ["pt"] = "E",
            ["nl"] = "G",
        },
        ["Col_LossShort"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "L",
            ["tr"] = "M",
            ["de"] = "N",
            ["fr"] = "D",
            ["es"] = "P",
            ["it"] = "S",
            ["pt"] = "D",
            ["nl"] = "V",
        },
        ["Col_WinRate"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Win %",
            ["tr"] = "Galibiyet %",
            ["de"] = "Sieg %",
            ["fr"] = "Victoire %",
            ["es"] = "% Victorias",
            ["it"] = "% Vittorie",
            ["pt"] = "% Vitórias",
            ["nl"] = "Winst %",
        },
        ["Col_Streak"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Streak",
            ["tr"] = "Seri",
            ["de"] = "Serie",
            ["fr"] = "Série",
            ["es"] = "Racha",
            ["it"] = "Striscia",
            ["pt"] = "Sequência",
            ["nl"] = "Reeks",
        },
        ["Col_Record"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "W–D–L",
            ["tr"] = "G–B–M",
            ["de"] = "S–U–N",
            ["fr"] = "V–N–D",
            ["es"] = "G–E–P",
            ["it"] = "V–P–S",
            ["pt"] = "V–E–D",
            ["nl"] = "W–G–V",
        },
        ["Col_LastPlayed"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Last played (UTC)",
            ["tr"] = "Son maç (UTC)",
            ["de"] = "Zuletzt gespielt (UTC)",
            ["fr"] = "Dernier match (UTC)",
            ["es"] = "Último partido (UTC)",
            ["it"] = "Ultima partita (UTC)",
            ["pt"] = "Última partida (UTC)",
            ["nl"] = "Laatst gespeeld (UTC)",
        },
        ["Register_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Create an account",
            ["tr"] = "Hesap oluştur",
            ["de"] = "Konto erstellen",
            ["fr"] = "Créer un compte",
            ["es"] = "Crear una cuenta",
            ["it"] = "Crea un account",
            ["pt"] = "Criar uma conta",
            ["nl"] = "Maak een account aan",
        },
        ["Register_GameIdHint"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "The username you type in the game.",
            ["tr"] = "Oyunda gireceğiniz kullanıcı adı.",
            ["de"] = "Der Benutzername, den Sie im Spiel eingeben.",
            ["fr"] = "Le nom d'utilisateur que vous tapez dans le jeu.",
            ["es"] = "El nombre de usuario que escribes en el juego.",
            ["it"] = "Il nome utente che digiti nel gioco.",
            ["pt"] = "O nome de usuário que você digita no jogo.",
            ["nl"] = "De gebruikersnaam die je in het spel typt.",
        },
        ["Register_PasswordHint"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Printable ASCII only, at most 16 characters — the game cannot reproduce anything wider.",
            ["tr"] = "Yalnızca standart karakterler, en fazla 16 karakter.",
            ["de"] = "Nur druckbares ASCII, maximal 16 Zeichen.",
            ["fr"] = "ASCII imprimable uniquement, maximum 16 caractères.",
            ["es"] = "Solo ASCII imprimible, máximo 16 caracteres.",
            ["it"] = "Solo ASCII stampabile, massimo 16 caratteri.",
            ["pt"] = "Apenas ASCII imprimível, no máximo 16 caracteres.",
            ["nl"] = "Alleen afdrukbare ASCII, maximaal 16 tekens.",
        },
        ["Register_ConfirmPassword"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Confirm password",
            ["tr"] = "Şifreyi onayla",
            ["de"] = "Passwort bestätigen",
            ["fr"] = "Confirmer le mot de passe",
            ["es"] = "Confirmar contraseña",
            ["it"] = "Conferma password",
            ["pt"] = "Confirmar senha",
            ["nl"] = "Wachtwoord bevestigen",
        },
        ["Register_Serial"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Serial",
            ["tr"] = "Seri Numarası",
            ["de"] = "Seriennummer",
            ["fr"] = "Numéro de série",
            ["es"] = "Número de serie",
            ["it"] = "Numero di serie",
            ["pt"] = "Número de série",
            ["nl"] = "Serienummer",
        },
        ["Register_SerialHint"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "The product serial the game was installed with.",
            ["tr"] = "Oyunun kurulduğu ürün seri numarası.",
            ["de"] = "Die Seriennummer, mit der das Spiel installiert wurde.",
            ["fr"] = "Le numéro de série avec lequel le jeu a été installé.",
            ["es"] = "El número de serie con el que se instaló el juego.",
            ["it"] = "Il numero di serie con cui è stato installato il gioco.",
            ["pt"] = "O número de série com o qual o jogo foi instalado.",
            ["nl"] = "Het serienummer waarmee het spel is geïnstalleerd.",
        },
        ["Register_Submit"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Create account",
            ["tr"] = "Hesap oluştur",
            ["de"] = "Konto erstellen",
            ["fr"] = "Créer le compte",
            ["es"] = "Crear cuenta",
            ["it"] = "Crea account",
            ["pt"] = "Criar conta",
            ["nl"] = "Account aanmaken",
        },
        ["Profile_OnlineNow"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Online Now",
            ["tr"] = "Şu An Çevrim İçi",
            ["de"] = "Jetzt online",
            ["fr"] = "En ligne",
            ["es"] = "En línea ahora",
            ["it"] = "Ora online",
            ["pt"] = "Online agora",
            ["nl"] = "Nu online",
        },
        ["Profile_Offline"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Offline",
            ["tr"] = "Çevrim Dışı",
            ["de"] = "Offline",
            ["fr"] = "Hors ligne",
            ["es"] = "Desconectado",
            ["it"] = "Offline",
            ["pt"] = "Offline",
            ["nl"] = "Offline",
        },
        ["Profile_MemberSince"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Member since",
            ["tr"] = "Kayıt tarihi",
            ["de"] = "Mitglied seit",
            ["fr"] = "Membre depuis",
            ["es"] = "Miembro desde",
            ["it"] = "Membro da",
            ["pt"] = "Membro desde",
            ["nl"] = "Lid sinds",
        },
        ["Profile_LastPlayed"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Last played",
            ["tr"] = "Son maç",
            ["de"] = "Zuletzt gespielt",
            ["fr"] = "Dernier match",
            ["es"] = "Último partido",
            ["it"] = "Ultima partita",
            ["pt"] = "Última partida",
            ["nl"] = "Laatst gespeeld",
        },
        ["Stat_Rating"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Rating",
            ["tr"] = "Puan Derecesi",
            ["de"] = "Wertung",
            ["fr"] = "Note",
            ["es"] = "Puntuación",
            ["it"] = "Punteggio",
            ["pt"] = "Classificação",
            ["nl"] = "Rating",
        },
        ["Stat_RankPosition"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Rank Position",
            ["tr"] = "Sıralama",
            ["de"] = "Rangplatz",
            ["fr"] = "Position au classement",
            ["es"] = "Posición de ranking",
            ["it"] = "Posizione",
            ["pt"] = "Posição",
            ["nl"] = "Rangpositie",
        },
        ["Stat_Points"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Points",
            ["tr"] = "Puan",
            ["de"] = "Punkte",
            ["fr"] = "Points",
            ["es"] = "Puntos",
            ["it"] = "Punti",
            ["pt"] = "Pontos",
            ["nl"] = "Punten",
        },
        ["Stat_SeasonPts"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Season pts",
            ["tr"] = "Sezon puanı",
            ["de"] = "Saisonpunkte",
            ["fr"] = "Pts saison",
            ["es"] = "Pts temporada",
            ["it"] = "Pti stagione",
            ["pt"] = "Pts temporada",
            ["nl"] = "Seizoensptn",
        },
        ["Stat_WeekPts"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Week pts",
            ["tr"] = "Haftalık puan",
            ["de"] = "Wochenpunkte",
            ["fr"] = "Pts semaine",
            ["es"] = "Pts semana",
            ["it"] = "Pti settimana",
            ["pt"] = "Pts semana",
            ["nl"] = "Weekptn",
        },
        ["Stat_Matches"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Matches",
            ["tr"] = "Maçlar",
            ["de"] = "Spiele",
            ["fr"] = "Matchs",
            ["es"] = "Partidos",
            ["it"] = "Partite",
            ["pt"] = "Partidas",
            ["nl"] = "Wedstrijden",
        },
        ["Stat_Wins"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Wins",
            ["tr"] = "Galibiyet",
            ["de"] = "Siege",
            ["fr"] = "Victoires",
            ["es"] = "Victorias",
            ["it"] = "Vittorie",
            ["pt"] = "Vitórias",
            ["nl"] = "Overwinningen",
        },
        ["Stat_Draws"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Draws",
            ["tr"] = "Beraberlik",
            ["de"] = "Unentschieden",
            ["fr"] = "Nuls",
            ["es"] = "Empates",
            ["it"] = "Pareggi",
            ["pt"] = "Empates",
            ["nl"] = "Gelijkspel",
        },
        ["Stat_Losses"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Losses",
            ["tr"] = "Mağlubiyet",
            ["de"] = "Niederlagen",
            ["fr"] = "Défaites",
            ["es"] = "Derrotas",
            ["it"] = "Sconfitte",
            ["pt"] = "Derrotas",
            ["nl"] = "Nederlagen",
        },
        ["Stat_WinRate"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Win %",
            ["tr"] = "Galibiyet %",
            ["de"] = "Sieg %",
            ["fr"] = "Victoire %",
            ["es"] = "% Victorias",
            ["it"] = "% Vittorie",
            ["pt"] = "% Vitórias",
            ["nl"] = "Winst %",
        },
        ["Stat_GoalsFor"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Goals",
            ["tr"] = "Goller",
            ["de"] = "Tore",
            ["fr"] = "Buts",
            ["es"] = "Goles",
            ["it"] = "Gol",
            ["pt"] = "Gols",
            ["nl"] = "Doelpunten",
        },
        ["Stat_GoalsAgainst"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Conceded",
            ["tr"] = "Yenen",
            ["de"] = "Gegentore",
            ["fr"] = "Encaissés",
            ["es"] = "Encajados",
            ["it"] = "Subiti",
            ["pt"] = "Sofridos",
            ["nl"] = "Tegendoelpunten",
        },
        ["Stat_GoalDiff"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Diff",
            ["tr"] = "Averaj",
            ["de"] = "Diff",
            ["fr"] = "Diff",
            ["es"] = "Dif",
            ["it"] = "Diff",
            ["pt"] = "Saldo",
            ["nl"] = "Saldo",
        },
        ["Stat_Streak"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Streak",
            ["tr"] = "Seri",
            ["de"] = "Serie",
            ["fr"] = "Série",
            ["es"] = "Racha",
            ["it"] = "Striscia",
            ["pt"] = "Sequência",
            ["nl"] = "Reeks",
        },
        ["Stat_BestStreak"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Best Streak",
            ["tr"] = "En İyi Seri",
            ["de"] = "Beste Serie",
            ["fr"] = "Meilleure série",
            ["es"] = "Mejor racha",
            ["it"] = "Miglior striscia",
            ["pt"] = "Melhor sequência",
            ["nl"] = "Beste reeks",
        },
        ["Stat_Disconnects"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Disconnects",
            ["tr"] = "Kopma",
            ["de"] = "Abbrüche",
            ["fr"] = "Déconnexions",
            ["es"] = "Desconexiones",
            ["it"] = "Disconnessioni",
            ["pt"] = "Desconexões",
            ["nl"] = "Verbroken",
        },
        ["Status_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "System & API Status",
            ["tr"] = "Sistem & API Durumu",
            ["de"] = "System- & API-Status",
            ["fr"] = "État du système et de l'API",
            ["es"] = "Estado del Sistema y API",
            ["it"] = "Stato del Sistema e API",
            ["pt"] = "Status do Sistema e API",
            ["nl"] = "Systeem- & API-status",
        },
        ["Status_Subtitle"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Live health telemetry and port monitoring for ELX11 game servers and APIs.",
            ["tr"] = "ELX11 oyun sunucuları ve API'leri için anlık sağlık ve port telemetrisi.",
            ["de"] = "Live-Telemetrie und Port-Überwachung für ELX11-Server und APIs.",
            ["fr"] = "Télémétrie en direct et surveillance des ports pour les serveurs et API ELX11.",
            ["es"] = "Telemetría en vivo y monitoreo de puertos para servidores y APIs de ELX11.",
            ["it"] = "Telemetria in tempo reale e monitoraggio delle porte per i server e le API ELX11.",
            ["pt"] = "Telemetria ao vivo e monitoramento de portas para servidores e APIs do ELX11.",
            ["nl"] = "Live gezondheidstelemetrie en poortbewaking voor ELX11 spelservers en API's.",
        },
        ["Status_AllOperational"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "All Systems Operational",
            ["tr"] = "Tüm Sistemler Çalışıyor",
            ["de"] = "Alle Systeme betriebsbereit",
            ["fr"] = "Tous les systèmes sont opérationnels",
            ["es"] = "Todos los sistemas operativos",
            ["it"] = "Tutti i sistemi operativi",
            ["pt"] = "Todos os sistemas operacionais",
            ["nl"] = "Alle systemen operationeel",
        },
        ["Status_Degraded"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "System Degradation Detected",
            ["tr"] = "Sistemde Sorun Algılandı",
            ["de"] = "Systembeeinträchtigung erkannt",
            ["fr"] = "Dégradation du système détectée",
            ["es"] = "Degradación del sistema detectada",
            ["it"] = "Rilevato degrado del sistema",
            ["pt"] = "Degradação do sistema detectada",
            ["nl"] = "Systeemverslechtering gedetecteerd",
        },
        ["Status_Uptime"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Server Uptime",
            ["tr"] = "Sunucu Çalışma Süresi",
            ["de"] = "Server-Betriebszeit",
            ["fr"] = "Disponibilité du serveur",
            ["es"] = "Tiempo de actividad",
            ["it"] = "Tempo di attività",
            ["pt"] = "Tempo de atividade",
            ["nl"] = "Server-uptime",
        },
        ["Status_Ram"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Process RAM",
            ["tr"] = "Bellek Kullanımı",
            ["de"] = "Prozess-RAM",
            ["fr"] = "RAM du processus",
            ["es"] = "RAM del proceso",
            ["it"] = "RAM del processo",
            ["pt"] = "RAM do processo",
            ["nl"] = "Proces-RAM",
        },
        ["Status_Services"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Services Monitored",
            ["tr"] = "İzlenen Servisler",
            ["de"] = "Überwachte Dienste",
            ["fr"] = "Services surveillés",
            ["es"] = "Servicios monitoreados",
            ["it"] = "Servizi monitorati",
            ["pt"] = "Serviços monitorados",
            ["nl"] = "Gemonitorde services",
        },
        ["Pager_PerPage"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Per page",
            ["tr"] = "Sayfa başına",
            ["de"] = "Pro Seite",
            ["fr"] = "Par page",
            ["es"] = "Por página",
            ["it"] = "Per pagina",
            ["pt"] = "Por página",
            ["nl"] = "Per pagina",
        },
        ["Pager_Prev"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Previous page",
            ["tr"] = "Önceki sayfa",
            ["de"] = "Vorherige Seite",
            ["fr"] = "Page précédente",
            ["es"] = "Página anterior",
            ["it"] = "Pagina precedente",
            ["pt"] = "Página anterior",
            ["nl"] = "Vorige pagina",
        },
        ["Pager_Next"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Next page",
            ["tr"] = "Sonraki sayfa",
            ["de"] = "Nächste Seite",
            ["fr"] = "Page suivante",
            ["es"] = "Página siguiente",
            ["it"] = "Pagina successiva",
            ["pt"] = "Próxima página",
            ["nl"] = "Volgende pagina",
        },
        ["Terms_Title"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Terms of Service",
            ["tr"] = "Kullanım Koşulları",
            ["de"] = "Nutzungsbedingungen",
            ["fr"] = "Conditions d'utilisation",
            ["es"] = "Términos de Servicio",
            ["it"] = "Termini di Servizio",
            ["pt"] = "Termos de Serviço",
            ["nl"] = "Servicevoorwaarden",
        },
        ["Terms_Subtitle"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Last updated: October 2026 · ELX11 Community",
            ["tr"] = "Son güncelleme: Ekim 2026 · ELX11 Topluluğu",
            ["de"] = "Zuletzt aktualisiert: Oktober 2026 · ELX11-Community",
            ["fr"] = "Dernière mise à jour : octobre 2026 · Communauté ELX11",
            ["es"] = "Última actualización: octubre de 2026 · Comunidad ELX11",
            ["it"] = "Ultimo aggiornamento: ottobre 2026 · Community ELX11",
            ["pt"] = "Última atualização: outubro de 2026 · Comunidade ELX11",
            ["nl"] = "Laatst bijgewerkt: oktober 2026 · ELX11-community",
        },
        ["Lang_Select"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Language",
            ["tr"] = "Dil",
            ["de"] = "Sprache",
            ["fr"] = "Langue",
            ["es"] = "Idioma",
            ["it"] = "Lingua",
            ["pt"] = "Idioma",
            ["nl"] = "Taal",
        },
    };

    public string this[string key] => Get(key, key);

    public string this[string key, string fallback] => Get(key, fallback);

    public string CurrentCulture => GetCurrentLanguageCode();

    public LanguageInfo CurrentLanguage => CultureCatalog.Resolve(CurrentCulture);

    public IReadOnlyList<LanguageInfo> SupportedLanguages => CultureCatalog.SupportedLanguages;

    private string GetCurrentLanguageCode()
    {
        var context = http.HttpContext;
        if (context is null)
            return CultureCatalog.DefaultCulture;

        // 1. Check query string: ?lang=tr
        if (context.Request.Query.TryGetValue("lang", out var queryLang) && CultureCatalog.IsSupported(queryLang))
            return queryLang.ToString().Trim().ToLowerInvariant();

        // 2. Check cookie: "lang" or ".AspNetCore.Culture"
        if (context.Request.Cookies.TryGetValue("lang", out var cookieLang) && CultureCatalog.IsSupported(cookieLang))
            return cookieLang.Trim().ToLowerInvariant();

        if (context.Request.Cookies.TryGetValue(".AspNetCore.Culture", out var aspCulture))
        {
            var match = ExtractCulture(aspCulture);
            if (CultureCatalog.IsSupported(match))
                return match;
        }

        // 3. Check Accept-Language header
        var accept = context.Request.Headers.AcceptLanguage.ToString();
        if (!string.IsNullOrEmpty(accept))
        {
            var parts = accept.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                var langTag = part.Split(';')[0].Trim();
                var primary = langTag.Split('-')[0].ToLowerInvariant();
                if (CultureCatalog.IsSupported(primary))
                    return primary;
            }
        }

        return CultureCatalog.DefaultCulture;
    }

    private static string ExtractCulture(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        if (raw.Contains("c=") || raw.Contains("uic="))
        {
            foreach (var segment in raw.Split('|'))
            {
                var kv = segment.Split('=');
                if (kv.Length == 2 && (kv[0] == "c" || kv[0] == "uic"))
                    return kv[1].Trim().ToLowerInvariant();
            }
        }
        return raw.Trim().ToLowerInvariant();
    }

    private string Get(string key, string fallback)
    {
        var lang = CurrentCulture;
        if (Translations.TryGetValue(key, out var langDict))
        {
            if (langDict.TryGetValue(lang, out var translated))
                return translated;
            if (langDict.TryGetValue(CultureCatalog.DefaultCulture, out var defaultTrans))
                return defaultTrans;
        }
        return fallback;
    }
}
