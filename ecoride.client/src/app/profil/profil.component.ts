import { Component } from '@angular/core';
import { AuthService } from '../authentification/auth.service';
import { HttpClient } from '@angular/common/http';

interface Marque {
  id: number;
  libelle: string;
}

interface Role {
  id: number;
  libelle: string;
}

interface Possede {
  utilisateur_id: number;
  role_id: string;
}

@Component({
  selector: 'app-profil',
  standalone: false,
  templateUrl: './profil.component.html',
  styleUrl: './profil.component.css'
})
export class ProfilComponent {
  user: any;
  isPassager = false;
  isChauffeur = false;
  public marque_existant: Marque[] = [];
  nouvelleMarqueActive = false;
  nouvelleMarqueLibelle = '';
  public rolesDispo: Role[] = [];

  energies: string[] = ["électrique", "diesel", "gazole","hybride"];


  voiture = {
    modele: '',
    immatriculation: '',
    energie: '',
    couleur: '',
    date_premiere_immatriculation: '',
    marque_id: 1,
    nb_place: 1,
    utilisateur_id: undefined,
    preference: {
      fumeur: false,
      animal: false,
      autre: ''
    }
  };

  constructor(private authService: AuthService, private http: HttpClient) {
    this.user = this.authService.currentUserValue;
    this.voiture.utilisateur_id = this.user?.id || 0; }

  ngOnInit() {
    this.http.get<Marque[]>('api/profil/marqueGet').subscribe(
      (data) => {
        this.marque_existant = data;
      },
      (error) => {
        console.error('Failed to load marque:', error);
      }
    );

    this.http.get<Role[]>('/api/Profil/roleGet').subscribe(data => {
      this.rolesDispo = data;
    });

  }

  submit() {
    const roles = [];
    const possede: Possede[] = [];

    if (this.isPassager) {
      const role = this.rolesDispo.find(r => r.libelle === 'passager');
      if (role) {
        possede.push({ utilisateur_id: this.user.id.toString(), role_id: role.id.toString() });
      }
    }

    if (this.isChauffeur) {
      const role = this.rolesDispo.find(r => r.libelle === 'chauffeur');
      if (role) {
        possede.push({ utilisateur_id: this.user.id.toString(), role_id: role.id.toString() });
      }
    }

    this.http.post('/api/Profil/roleSet', possede).subscribe({
      next: () => { console.log('Rôles envoyés avec succès'); alert("Rôles sélectionné avec succès"); },
      error: (err) => console.error('Erreur lors de l\'envoi des rôles', err)
    });


    const enregistrerVoiture = () => {
      this.voiture.utilisateur_id = this.user.id;

      this.http.post('/api/Profil/voiture', this.voiture).subscribe({
        next: () => { console.log("Voiture enregistrée"); alert("Voiture enregistrée"); },
        error: (err) => console.error("Erreur voiture :", err)
      });
    };

    if (this.isChauffeur) {
      if (this.nouvelleMarqueActive && this.nouvelleMarqueLibelle.trim() !== '') {
        // Ajouter la nouvelle marque
        this.http.post<number>('/api/Profil/marqueAdd', JSON.stringify(this.nouvelleMarqueLibelle), // on convertit en JSON string (avec guillemets)
          {
            headers: { 'Content-Type': 'application/json' }
          }
        ).subscribe({
          next: (id) => {
            this.voiture.marque_id = id;
            enregistrerVoiture();
          },
          error: (err) => console.error("Erreur ajout marque :", err)
        });
      } else {
        // Utiliser la marque existante
        enregistrerVoiture();
      }
    }
  }

}
