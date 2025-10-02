import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Utilisateur {
  id: number;
  nom: string;
  prenom: string;
  email: string;
  telephone: string;
  adresse: string;
  dateNaissance: string;
  pseudo: string;
  acces: string;
  autorisation?: string; // actif / suspendu
}

@Component({
  selector: 'app-suspendre-utilisateur',
  standalone: false,
  templateUrl: './suspendre-utilisateur.component.html',
  styleUrl: './suspendre-utilisateur.component.css'
})
export class SuspendreUtilisateurComponent {
  utilisateurs: Utilisateur[] = [];
  filtre: string = '';
  isLoading = true;
  errorMessage = '';

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.chargerUtilisateurs();
  }

  get utilisateursFiltres(): Utilisateur[] {
    const filtreLower = this.filtre.toLowerCase();
    return this.utilisateurs.filter(u =>
      u.nom.toLowerCase().includes(filtreLower) ||
      u.pseudo.toLowerCase().includes(filtreLower) ||
      u.email.toLowerCase().includes(filtreLower)
    );
  }

  chargerUtilisateurs() {
    this.isLoading = true;
    this.http.get<Utilisateur[]>('/api/Utilisateur/GetUtilisateurAll')
      .subscribe({
        next: (data) => {
          this.utilisateurs = data.filter(u => u.acces?.toLowerCase() !== 'administrateur');
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = "Erreur lors du chargement des utilisateurs.";
          this.isLoading = false;
        }
      });
  }

  suspendreUtilisateur(utilisateur: Utilisateur) {
    const nouveauStatut = utilisateur.autorisation === 'suspendu' ? 'actif' : 'suspendu';

    this.http.put(`/api/Utilisateur/modifierAutorisation/${utilisateur.id}`, { autorisation: nouveauStatut })
      .subscribe({
        next: () => {
          utilisateur.autorisation = nouveauStatut;
        },
        error: () => {
          alert("Erreur lors de la modification.");
        }
      });
  }
}
