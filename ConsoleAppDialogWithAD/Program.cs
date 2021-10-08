using System;
using System.DirectoryServices;

namespace ConsoleAppDialogWithAD
{
  class Program
  {
    static void Main(string[] arguments)
    {
      const long ADS_UF_SCRIPT = 0x0001; // Le script de démarrage sera exécuté
      const long ADS_UF_ACCOUNTDISABLE = 0x0002; // Désactiver le compte
      const long ADS_UF_HOMEDIR_REQUIRED = 0x0008; // Nécessite un répertoire racine
      const long ADS_UF_LOCKOUT = 0x0010; // Le compte est verrouillé
      const long ADS_UF_PASSWD_NOTREQD = 0x0020; // Aucun mot de passe nécessaire
      const long ADS_UF_PASSWD_CANT_CHANGE = 0x0040; // L'utilisateur ne peut pas changer de mot de passe
      const long ADS_UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x0080; // Cryptage du mpd autorisé
      const long ADS_UF_TEMP_DUPLICATE_ACCOUNT = 0x0100; // Compte d'utilisateur local
      const long ADS_UF_NORMAL_ACCOUNT = 0x0200;
      Action<string> Display = Console.WriteLine;
      Display("Getting the list from an Active directory.");
      try
      {
        DirectoryEntry ldap = new DirectoryEntry("LDAP://yourAD", "username", "password");
        Display("Connecté à votre AD correctement");
        DirectorySearcher searcher = new DirectorySearcher(ldap);
        searcher.Filter = "(objectClass=user)";
        foreach (SearchResult result in searcher.FindAll())
        {
          // On récupère l'entrée trouvée lors de la recherche
          DirectoryEntry DirEntry = result.GetDirectoryEntry();
          //On peut maintenant afficher les informations désirées
          Display("Login : " + DirEntry.Properties["SAMAccountName"].Value);
          Display("Nom : " + DirEntry.Properties["sn"].Value);
          Display("Prénom : " + DirEntry.Properties["givenName"].Value);
          Display("Email : " + DirEntry.Properties["mail"].Value);
          Display("Tél : " + DirEntry.Properties["TelephoneNumber"].Value);
          Display("Description : " + DirEntry.Properties["description"].Value);
          Display("-------------------");
        }
      }
      catch (Exception exception)
      {
        Display("Pas de connexion à votre AD");
        Display($"Exception: {exception.Message}");
      }


      Display("Press any key to exit:");
      Console.ReadKey();
    }

    private bool ChangeADEntry(string path, string username, string password, string userToBeSearchedFor, string newPhoneNumber = "", string userDescription = "")
    {
      bool changeSuccessful = false;
      try
      {
        // Connexion à l'annuaire
        DirectoryEntry Ldap = new DirectoryEntry(path, username, password);
        // Nouvel objet pour instancier la recherche
        DirectorySearcher searcher = new DirectorySearcher(Ldap);
        // On modifie le filtre pour ne chercher que l'user dont le nom de login est TEST
        searcher.Filter = $"(SAMAccountName={userToBeSearchedFor})";
        // Pas de boucle foreach car on ne cherche qu'un user
        SearchResult result = searcher.FindOne();
        // On récupère l'objet trouvé lors de la recherche
        DirectoryEntry DirEntry = result.GetDirectoryEntry();
        // On modifie la propriété description de l'utilisateur TEST
        DirEntry.Properties["description"].Value = userDescription;
        // Et son numéro de téléphone
        DirEntry.Properties["TelephoneNumber"].Value = newPhoneNumber;
        // On envoie les changements à Active Directory
        DirEntry.CommitChanges();
        changeSuccessful = true;
      }
      catch (Exception)
      {
        changeSuccessful = false;
      }

      return changeSuccessful;
    }

    private bool AddADUser(string path, string username, string password, string newUserName, string newUserPassword)
    {
      bool changeSuccessful = false;
      try
      {
        // Connexion à l'annuaire
        DirectoryEntry Ldap = new DirectoryEntry(path, username, password);
        // Création du user Test User et initialisation de ses propriétés
        DirectoryEntry user = Ldap.Children.Add("cn=Test User", "user");
        user.Properties["SAMAccountName"].Add("testuser");
        user.Properties["sn"].Add("User");
        user.Properties["givenName"].Add("Test");
        user.Properties["description"].Add("Compte de test créé par le code");
        // On envoie les modifications au serveur
        user.CommitChanges();
        // On va maintenant lui définir son password. L'utilisateur doit avoir été créé
        // et sauvé avant de pouvoir faire cette étape
        user.Invoke("SetPassword", new object[] { newUserPassword });
        // On va maintenant activer le compte : ADS_UF_NORMAL_ACCOUNT
        user.Properties["userAccountControl"].Value = 0x0200;
        // On envoie les modifications au serveur
        user.CommitChanges();
        changeSuccessful = true;
      }
      catch (Exception)
      {
        changeSuccessful = false;
      }

      return changeSuccessful;
    }
  }
}
