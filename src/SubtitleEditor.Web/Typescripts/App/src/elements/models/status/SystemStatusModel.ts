
export default interface SystemStatusModel {
    asrKernelVersion?: string;
    captionMakerVersion?: string;
    videoSubtitleEditorVersion: string;
    totalWorkers?: number;
    availableWorkers?: number;
    asrStatus?: number;
    licenseExpiredTime?: string;

    storageLimit: number;
    streamFileLimit: number;
    storageLength: number;
    streamFileLength: number;

    activated: boolean;
    activationKeyPublisher: string;
    activatedTarget: string;
    calCount?: number;
    activationEnd: string;
}