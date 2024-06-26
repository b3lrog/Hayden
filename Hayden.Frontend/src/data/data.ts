export class InfoObject {
    apiEndpoint: string;
    rawEndpoint: string;
    hCaptchaSiteKey: string | null;
    maxGlobalUploadSize: number | null;
    siteName: string;
    quoteList: string[] | null;
    bannerFilename: string | null;
    newsItems: NewsItem[] | null;
    shiftJisArt: string | null;
    searchEnabled: boolean;
	compactBoardMode: boolean;
    thumbLinks: boolean;
    fileExpanding: boolean;
}

export interface NewsItem {
    Title: string;
    DateString: string;
    Content: string;
}

export class BoardModel {
    id: number;

    shortName: string;
    longName: string;
    category: string;
    isNSFW: boolean;
    isReadOnly: boolean;
}

export class BoardPageModel {
    threads: ThreadModel[];

    totalThreadCount: number;
    boardInfo: BoardModel;
}

export class ThreadModel {
    threadId: number;

    board: BoardModel;

    subject: string;

    lastModified: string;

    archived: boolean;
    deleted: boolean;

    omitted: number;

    posts: PostModel[];
}

export class PostModel {
    postId: number;

    contentHtml: string | null;
    contentRaw: string | null;

    author: string | null;
    email: string | null;
    tripcode: string | null;

    dateTime: string;

    deleted: boolean;

    embed: string | null;

    capcode: string | null;

    posterId: string | null;

    country: CountryModel | null;

    files: FileModel[];
}

export class CountryModel {
    code: string;
    name: string;
}

export class FileModel {
    fileId: number;

    md5Hash: Uint8Array;
    sha1Hash: Uint8Array;
    sha256Hash: Uint8Array;

    extension: string;

    imageWidth: number | null;
    imageHeight: number | null;

    fileSize: number;

    index: number;
    filename: string;

    spoiler: boolean;
    deleted: boolean;

    imageUrl: string;
    thumbnailUrl: string;
}

export enum ModeratorRole {
    Janitor = 1,
    Moderator = 2,
    Developer = 3,
    Admin = 4
}
