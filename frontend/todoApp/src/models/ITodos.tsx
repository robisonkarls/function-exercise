export interface ITodo {
    "id": number,
    "title": string,
    "status": string,
    "isArchived": boolean,
    "createdAtUtc": string,
    "updatedAtUtc": string | null
  }