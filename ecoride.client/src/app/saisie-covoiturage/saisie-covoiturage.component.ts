import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../authentification/auth.service'; // adapte le chemin si besoin


@Component({
  selector: 'app-saisie-covoiturage',
  standalone: false,
  templateUrl: './saisie-covoiturage.component.html',
  styleUrl: './saisie-covoiturage.component.css'
})
export class SaisieCovoiturageComponent implements OnInit {

  covoiturageForm!: FormGroup;
  voituresUtilisateur: any[] = [];
  ajoutNouvelleVoiture = false;

  utilisateurId!: number;
  loading = false;
  message = '';
  nouvelleVoiture: any = {};

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    const utilisateur = this.authService.currentUserValue; // remplace selon ton implémentation
    if (utilisateur && utilisateur.id) {
      this.utilisateurId = utilisateur.id;
      this.initForm();
      this.chargerVoitures();
    } else {
      this.message = "Utilisateur non connecté.";
    }
  }

  initForm() {
    this.covoiturageForm = this.fb.group({
      dateDepart: ['', Validators.required],
      heureDepart: ['', Validators.required],
      lieuDepart: ['', Validators.required],
      dateArrivee: ['', Validators.required],
      heureArrivee: ['', Validators.required],
      lieuArrivee: ['', Validators.required],
      nbPlace: [1, [Validators.required, Validators.min(1)]],
      prixPersonne: [2, [Validators.required, Validators.min(0)]],
      voitureId: ['', Validators.required],

      // Sous-formulaire voiture
      nouvelleVoiture: this.fb.group({
        marque: [''],
        modele: [''],
        immatriculation: ['']
        // ajoute d'autres champs si besoin
      })
    });
  }

  get nouvelleVoitureGroup(): FormGroup {
    return this.covoiturageForm.get('nouvelleVoiture') as FormGroup;
  }

  chargerVoitures() {
    this.http.get<any[]>(`/api/profil/voiture/${this.utilisateurId}`).subscribe({
      next: (voitures) => {
        this.voituresUtilisateur = voitures;
      },
      error: (err) => {
        console.error("Erreur lors du chargement des voitures :", err);
        this.message = "Impossible de charger les voitures.";
      }
    });
  }

  onVoitureChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.ajoutNouvelleVoiture = value === 'autre';
    if (this.ajoutNouvelleVoiture) {
      this.covoiturageForm.get('voitureId')?.setValue(null);
    }
  }

  onSubmit() {
    this.loading = true;

    if (this.ajoutNouvelleVoiture) {
      const nouvelleVoitureData = this.covoiturageForm.get('nouvelleVoiture')?.value;

      this.http.post(`/api/voiture/ajouter/${this.utilisateurId}`, nouvelleVoitureData).subscribe({
        next: (voiture: any) => {
          this.covoiturageForm.patchValue({ voitureId: voiture.id });
          this.envoyerCovoiturage();
        },
        error: (err) => {
          console.error("Erreur lors de l'ajout de la voiture :", err);
          this.message = "Erreur lors de l'ajout de la voiture.";
          this.loading = false;
        }
      });
    } else {
      this.envoyerCovoiturage();
    }
  }


  envoyerCovoiturage() {
    const covoiturage = this.covoiturageForm.value;

    this.http.post('/api/Covoiturage/Ajouter', covoiturage, { responseType: 'text' }).subscribe({
      next: (res) => {
        this.message = res;  // Affiche le message texte retourné par l'API
        this.covoiturageForm.reset();
        this.loading = false;
      },
      error: (err) => {
        console.error("Erreur lors de l'envoi :", err);
        this.message = `Erreur lors de l'envoi du covoiturage : ${err.message || err.statusText}`;
        this.loading = false;
      }
    });

  }
}
