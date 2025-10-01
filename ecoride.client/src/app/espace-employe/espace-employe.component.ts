import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { AuthService } from '../authentification/auth.service';

interface Avis {
  id: number;
  commentaire: string;
  note: number;
  statut: string;
}

interface TempAvis {
  avis: Avis;
  utilisateur_id: number;
  covoiturage_id: number;
}

interface Covoiturage {
  id?: number;
  dateDepart: string;
  lieuDepart: string;
  dateArrivee: string;
  lieuArrivee: string;
}

interface Utilisateur {
  id?: number;
  pseudo: string;
  email: string;
}

interface PbAvis {
  avis: Avis;
  passager: Utilisateur;
  chauffeur: Utilisateur;
  covoiturage: Covoiturage;
}

@Component({
  selector: 'app-espace-employe',
  standalone: false,
  templateUrl: './espace-employe.component.html',
  styleUrl: './espace-employe.component.css'
})
export class EspaceEmployeComponent {
  tempAvisList: TempAvis[] = [];
  problemeAvisList: PbAvis[] = [];

  constructor(private authService: AuthService, private http: HttpClient) { }

  ngOnInit(): void {
    this.loadAvis();
  }

  loadAvis() {
    this.http.get<TempAvis[]>('/api/Avis/Get/temp_avis').subscribe(
      (data) => {
        this.tempAvisList = data;
      },
      (error) => {
        console.error('Failed to load marque:', error);
      }
    );

    this.http.get<PbAvis[]>('/api/Avis/Get/Refuse/temp_avis').subscribe(
      (data) => {
        this.problemeAvisList = data;
      },
      (error) => {
        console.error('Failed to load marque:', error);
      }
    );
  }

  valider(id: number) {
    const item = this.tempAvisList.find(c => c.avis.id === id);
    if (!item) return;

    this.http.post('/api/Avis/Valider/temp_avis', item).subscribe({
      next: () => {
        console.log('Statut mis à jour : avis validé');
        item.avis.statut = 'valider';
      },
      error: (err) => console.error('Erreur lors de la mise à jour', err)
    });
  }

  nouvelleValidation(id: number) {
    const item = this.problemeAvisList.find(c => c.avis.id === id);
    if (!item) return;


    this.http.post('/api/Avis/temp_avis_refuse/validation', item).subscribe({
      next: () => {
        console.log('Statut mis à jour : avis validé');
        item.avis.statut = 'valider';
      },
      error: (err) => console.error('Erreur lors de la mise à jour', err)
    });
  }

  refuser(id: number) {
    const item = this.tempAvisList.find(c => c.avis.id === id);
    if (!item) return;

    this.http.post('/api/Avis/Refuser/temp_avis', item).subscribe({
      next: () => {
        console.log('Statut mis à jour : avis refusé');
        item.avis.statut = 'refuser';
      },
      error: (err) => console.error('Erreur lors de la mise à jour', err)
    });
  }
}
