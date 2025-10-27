import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap, map } from 'rxjs';

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


@Component({
  selector: 'app-ajouter-employe',
  standalone: false,
  templateUrl: './ajouter-employe.component.html',
  styleUrl: './ajouter-employe.component.css'
})
export class AjouterEmployeComponent {
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
      this.http.post('/api/utilisateur/PostUser?statut=employe', utilisateurToSend).subscribe(response => {
        console.log("employé créé :", response);
      });
    } else {
      this.http.post('/api/utilisateur/PostUser?statut=employe', this.form).subscribe(response => {
        console.log("employé créé :", response);
      });
    }
  }


  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.form.photo = file;
    }
  }

  isFormValid(): boolean {
    return (
      this.form.nom.trim() !== "" &&
      this.form.prenom.trim() !== "" &&
      this.form.email.trim() !== "" &&
      this.form.password.trim() !== "" &&
      this.form.telephone.trim() !== "" &&
      this.form.adresse.trim() !== "" &&
      this.form.dateNaissance.trim() !== "" &&
      this.form.pseudo.trim() !== ""
    );
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
