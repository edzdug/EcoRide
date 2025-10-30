import { Component } from '@angular/core';
import { AuthService } from '../authentification/auth.service';
import { HttpClient } from '@angular/common/http';
import { delay, forkJoin } from 'rxjs';

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
  public rolesUser: string[] = [];
  voituresUser: any;
  energies: string[] = ["Electrique", "Diesel", "Essence","Hybride"];
  message: string | undefined;

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
    forkJoin({
      marques: this.http.get<Marque[]>('/api/Profil/marqueGet'),
      roles: this.http.get<Role[]>('/api/Profil/roleGet'),
      rolesUser: this.http.get<string[]>(`/api/Profil/roleUserGet/${this.user.id}`),
      voiture: this.http.get<any[]>(`/api/Profil/voiture/${this.user.id}`)
    }).subscribe({
      next: ({ marques, roles, rolesUser, voiture }) => {
        this.marque_existant = marques;
        this.rolesDispo = roles;
        this.rolesUser = rolesUser;
        this.voituresUser = voiture.map(v => {
          const m = this.marque_existant.find(x => x.id === v.marque_id);
          return { ...v, marque_libelle: m ? m.libelle : 'Marque inconnue' };
        });
                
      },
      error: (err) => console.error('Erreur chargement profil :', err)
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
      next: () => console.log('Rôles envoyés avec succès'),
      error: (err) => console.error('Erreur lors de l\'envoi des rôles', err)
    });


    const enregistrerVoiture = () => {
      this.voiture.utilisateur_id = this.user.id;

      this.http.post('/api/Profil/voiture', this.voiture).subscribe({
        next: (data) => {
          console.log("Voiture enregistrée");
          if (data) this.message = "Enregistrement de la voiture réussi";
          else this.message = "Tentative d'insertion d'une immatriculation déjà existante";
        },
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
    delay(3000);
    this.recharger();
  }

  recharger() {
    this.http.get<any[]>(`/api/Profil/voiture/${this.user.id}`).subscribe(data => {
      this.voituresUser = data.map(v => {
        const m = this.marque_existant.find(x => x.id === v.marque_id);
        return { ...v, marque_libelle: m?.libelle || 'Marque inconnue' };
      });
    });
  }
}
