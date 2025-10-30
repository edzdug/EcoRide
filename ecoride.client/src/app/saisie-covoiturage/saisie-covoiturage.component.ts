import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../authentification/auth.service'; // adapte le chemin si besoin
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap, map } from 'rxjs';


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

  // 🔹 Ajout de deux contrôles distincts pour les adresses
  lieuDepartCtrl = new FormControl<string>('');
  lieuArriveeCtrl = new FormControl<string>('');

  // 🔹 Tableaux de suggestions indépendants
  suggestionsDepart: string[] = [];
  suggestionsArrivee: string[] = [];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    const utilisateur = this.authService.currentUserValue; 
    if (utilisateur && utilisateur.id) {
      this.utilisateurId = utilisateur.id;
      this.initForm();
      this.chargerVoitures();
      this.initAutocomplete();
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

      /*// Sous-formulaire voiture
      nouvelleVoiture: this.fb.group({
        modele: '',
        immatriculation: '',
        energie: '',
        couleur: '',
        date_premiere_immatriculation: '',
        marque_id: 1,
        nb_place: 0,
        utilisateur_id: undefined,
        preference: {
          fumeur: false,
          animal: false,
          autre: ''
        }
      })*/
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
      alert('Vous allez être redirigé vers votre profil pour ajouter une nouvelle voiture.');
      this.router.navigate(['/profil']);
    }
  }

  onSubmit() {
    this.loading = true;

    if (this.ajoutNouvelleVoiture) {
      const nouvelleVoitureData = this.covoiturageForm.get('nouvelleVoiture')?.value;

      this.http.post('/api/Profil/voiture', nouvelleVoitureData).subscribe({
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

    this.http.post(`/api/Covoiturage/Ajouter?utilisateurId=${this.utilisateurId}`, covoiturage, { responseType: 'text' }).subscribe({
      next: (res) => {
        this.message = "Covoiturage ajouté avec succès";  
        this.covoiturageForm.reset();
        this.loading = false;
      },
      error: (err) => {
        console.error("Erreur lors de l'envoi :", err);
        this.message = `Erreur lors de l'envoi du covoiturage`;
        this.loading = false;
      }
    });

  }

  // 🔹 Initialisation des suggestions automatiques
  initAutocomplete() {
    // Auto-complétion départ
    this.lieuDepartCtrl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(value => this.getSuggestions(value ?? ''))
    ).subscribe(results => this.suggestionsDepart = results);

    // Auto-complétion arrivée
    this.lieuArriveeCtrl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(value => this.getSuggestions(value ?? ''))
    ).subscribe(results => this.suggestionsArrivee = results);
  }

  // 🔹 Appel API adresse
  getSuggestions(query: string) {
    if (!query || query.length < 3) return [];
    const url = `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(query)}&limit=5`;
    return this.http.get<any>(url).pipe(
      map(res => res.features.map((f: any) => f.properties.label))
    );
  }

  // 🔹 Sélection d'une suggestion départ
  choisirDepart(adresse: string) {
    this.covoiturageForm.patchValue({ lieuDepart: adresse });
    this.lieuDepartCtrl.setValue(adresse, { emitEvent: false });
    this.suggestionsDepart = [];
  }

  // 🔹 Sélection d'une suggestion arrivée
  choisirArrivee(adresse: string) {
    this.covoiturageForm.patchValue({ lieuArrivee: adresse });
    this.lieuArriveeCtrl.setValue(adresse, { emitEvent: false });
    this.suggestionsArrivee = [];
  }


}
