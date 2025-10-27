import { HttpClient } from '@angular/common/http';
import { Component, Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, map, switchMap } from 'rxjs';

type Form = {
  nom: string;
  prenom: string;
  email: string;
  password: string;
  telephone: string;
  adresse: string;
  dateNaissance: string;
  photo: Blob | undefined;
  pseudo: string;
};

@Injectable({ providedIn: 'root' })
export class FormDataService {
  private _form: Form | null = null;

  setForm(form: Form) {
    this._form = form;
  }

  getForm(): Form | null {
    return this._form;
  }
}

@Component({
  selector: 'app-inscription',
  standalone: false,
  templateUrl: './inscription.component.html',
  styleUrl: './inscription.component.css'
})
export class InscriptionComponent {
  form: Form = {
    nom: "",
    prenom: "",
    email: "",
    password: "",
    telephone: "",
    adresse: "",
    dateNaissance: "",
    photo: undefined,
    pseudo: ""
  };
  adresseCtrl = new FormControl<string>('');
  suggestions: string[] = [];
  constructor(
    private http: HttpClient,
    private router: Router,
    private formDataService: FormDataService
  ) { }

  ngOnInit() {
    this.adresseCtrl.valueChanges.pipe(
      debounceTime(400),             // ⏳ attend 400ms entre les frappes
      distinctUntilChanged(),         // évite les appels inutiles
      switchMap(value => this.getSuggestions(value ?? ''))
    ).subscribe(results => this.suggestions = results);
  }

  async envoie() {
    if (this.form.photo) {
      const base64Photo = await blobToBase64(this.form.photo);
      const utilisateurToSend = { ...this.form, photo: base64Photo };
      this.http.post('/api/utilisateur/PostUser?statut=utilisateur', utilisateurToSend)
        .subscribe({
          next: (response) => {
            console.log("Utilisateur créé :", response);
            this.router.navigate(['/login']);
          },
          error: (err) => {
            console.error("Erreur lors de l'inscription :", err);
            alert("Erreur d'inscription : " + (err?.error || err?.message));
          }
        });

    } else {
      this.http.post('/api/utilisateur/PostUser?statut=utilisateur', this.form)
  .subscribe({
    next: (response) => {
      console.log("Utilisateur créé :", response);
      this.router.navigate(['/login']);
    },
    error: (err) => {
      console.error("Erreur lors de l'inscription :", err);
      alert("Erreur d'inscription : " + (err?.error || err?.message));
    }
  });

    }
  }


  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.form.photo = file;
    }
  }


  getSuggestions(query: string) {
    if (!query || query.length < 3) return []; // n'appelle pas trop tôt
    const url = `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(query)}&limit=5`;
    return this.http.get<any>(url).pipe(
      // on mappe les résultats pour n’extraire que les noms
      map(res => res.features.map((f: any) => f.properties.label))
    );
  }

  choisirAdresse(adresse: string) {
    this.adresseCtrl.setValue(adresse);
    this.suggestions = [];
  }

}
function blobToBase64(blob: Blob): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = reject;
    reader.onload = () => {
      resolve(reader.result as string); // contient le base64
    };
    reader.readAsDataURL(blob); // convertit le blob en data URI base64
  });
}


