# H??ng d?n C?u hình Image Processing

## T?ng quan
SlideGenerator Framework h? tr? ba ch? ?? c?t ?nh (ROI):

1. **Center** - C?t ??n gi?n ? gi?a ?nh
2. **Prominent** - Tìm và c?t vùng n?i b?t nh?t s? d?ng phân tích spectral residual
3. **Attention** (M?i) - K?t h?p nh?n di?n khuôn m?t v?i ?? n?i b?t ?? t?i ?u cho ?nh ng??i

## C?u hình Attention Mode

Ch? ?? Attention s? d?ng các tham s? sau trong file `backend.config.yaml`:

### Cài ??t Nh?n di?n Khuôn m?t (`image.face`)

#### `face.confidence` (m?c ??nh: 0.6)
- **Mô t?**: Ng??ng ?? tin c?y t?i thi?u (0-1) ?? ch?p nh?n m?t khuôn m?t ???c phát hi?n
- **Giá tr? th?p (0.3-0.5)**: Phát hi?n nhi?u khuôn m?t h?n, có th? bao g?m c? false positives
- **Giá tr? cao (0.7-0.9)**: Ch? ch?p nh?n khuôn m?t có ?? tin c?y cao, ít false positives
- **Khuy?n ngh?**: 0.6 cho h?u h?t tr??ng h?p, 0.7-0.8 n?u mu?n chính xác h?n

#### Face Padding (m?c ??nh m?i bên: 0.15)
Padding xung quanh khuôn m?t (0-1), tính theo kích th??c khuôn m?t. Có th? c?u hình **riêng bi?t cho t?ng h??ng**:

**`face.padding_top`**
- **Mô t?**: Padding phía trên khuôn m?t
- **Ví d?**: 0.15 = thêm 15% chi?u cao khuôn m?t phía trên
- **Khuy?n ngh?**: 0.15-0.25 ?? bao g?m tóc và trán

**`face.padding_bottom`**
- **Mô t?**: Padding phía d??i khuôn m?t
- **Ví d?**: 0.15 = thêm 15% chi?u cao khuôn m?t phía d??i
- **Khuy?n ngh?**: 0.15-0.20 ?? bao g?m c? và vai

**`face.padding_left`**
- **Mô t?**: Padding bên trái khuôn m?t
- **Ví d?**: 0.15 = thêm 15% chi?u r?ng khuôn m?t bên trái
- **Khuy?n ngh?**: 0.15-0.20 cân ??i hai bên

**`face.padding_right`**
- **Mô t?**: Padding bên ph?i khuôn m?t
- **Ví d?**: 0.15 = thêm 15% chi?u r?ng khuôn m?t bên ph?i
- **Khuy?n ngh?**: 0.15-0.20 cân ??i hai bên

**L?u ý**: Các giá tr? padding có th? khác nhau gi?a các h??ng ?? t?o composition t?t h?n (ví d?: top l?n h?n bottom cho ?nh chân dung).

#### `face.union_all` (m?c ??nh: true)
- **Mô t?**: Cách x? lý khi phát hi?n nhi?u khuôn m?t
- **true**: H?p t?t c? khuôn m?t thành m?t vùng ROI duy nh?t (phù h?p cho ?nh nhóm)
- **false**: Ch? ch?n khuôn m?t t?t nh?t (?i?m cao nh?t, l?n nh?t) (phù h?p cho ?nh chân dung ??n)
- **Khuy?n ngh?**: true cho ?nh t?ng th?, false cho ?nh cá nhân

### Cài ??t Saliency (`image.saliency`)

#### Saliency Padding (m?c ??nh m?i bên: 0.0)
Padding xung quanh vùng n?i b?t (0-1), tính theo kích th??c c?a s? c?t. C?ng có th? c?u hình **riêng bi?t cho t?ng h??ng**:

**`saliency.padding_top`**, **`saliency.padding_bottom`**, **`saliency.padding_left`**, **`saliency.padding_right`**
- **Mô t?**: Padding theo t?ng h??ng xung quanh vùng n?i b?t
- **0.0**: Không có padding, c?t chính xác theo vùng n?i b?t + khuôn m?t
- **> 0.0**: M? r?ng vùng ROI ?? bao g?m nhi?u context h?n
- **Khuy?n ngh?**: Gi? ? 0.0 trong h?u h?t tr??ng h?p, ho?c t?ng nh? (0.05-0.1) n?u mu?n thêm context

## Ví d? C?u hình

### Chân dung cá nhân - ?? chính xác cao
```yaml
image:
  face:
    confidence: 0.75
    padding_top: 0.25        # Nhi?u padding trên cho tóc
    padding_bottom: 0.15     # Ít h?n d??i
    padding_left: 0.20
    padding_right: 0.20
    union_all: false
  saliency:
    padding_top: 0.0
    padding_bottom: 0.0
    padding_left: 0.0
    padding_right: 0.0
```

### ?nh nhóm - Bao g?m t?t c?
```yaml
image:
  face:
    confidence: 0.55
    padding_top: 0.15
    padding_bottom: 0.15
    padding_left: 0.15
    padding_right: 0.15
    union_all: true
  saliency:
    padding_top: 0.0
    padding_bottom: 0.0
    padding_left: 0.0
    padding_right: 0.0
```

### ?nh ngh? thu?t - Context b?t ??i x?ng
```yaml
image:
  face:
    confidence: 0.65
    padding_top: 0.30        # Nhi?u không gian trên
    padding_bottom: 0.20
    padding_left: 0.25       # Nhi?u bên trái (rule of thirds)
    padding_right: 0.15      # Ít bên ph?i
    union_all: true
  saliency:
    padding_top: 0.05
    padding_bottom: 0.05
    padding_left: 0.05
    padding_right: 0.05
```

### ?nh n?m ngang - T?i ?u cho landscape
```yaml
image:
  face:
    confidence: 0.60
    padding_top: 0.10        # Ít padding trên/d??i
    padding_bottom: 0.10
    padding_left: 0.25       # Nhi?u padding trái/ph?i cho không gian
    padding_right: 0.25
    union_all: true
  saliency:
    padding_top: 0.0
    padding_bottom: 0.0
    padding_left: 0.0
    padding_right: 0.0
```

## L?u ý

1. **Hi?u n?ng**: Ch? ?? Attention c?n thêm th?i gian x? lý do ph?i ch?y mô hình nh?n di?n khuôn m?t. Mô hình ???c cache sau l?n ??u tiên.

2. **Fallback**: N?u không phát hi?n ???c khuôn m?t, h? th?ng t? ??ng fallback v? ch? ?? Prominent (ch? dùng saliency).

3. **B? nh?**: Mô hình nh?n di?n khuôn m?t chi?m ~10-20MB RAM khi ???c load.

4. **Thread-safe**: Mô hình ???c kh?i t?o m?t l?n và có th? dùng chung cho nhi?u requests.

5. **Padding b?t ??i x?ng**: S? d?ng padding khác nhau cho các h??ng ?? t?o composition theo quy t?c nhi?p ?nh (rule of thirds, golden ratio, etc.)

## API s? d?ng

### Trong Infrastructure (T? ??ng t? config)
```csharp
// SlideGenerator và ImageService t? ??ng ??c config
// Không c?n code thêm
```

### Trong Framework (Tùy ch?nh th? công)
```csharp
using SlideGenerator.Framework.Image;
using SlideGenerator.Framework.Image.Configs;
using SlideGenerator.Framework.Image.Models;

// T?o RoiOptions v?i padding tùy ch?nh theo t?ng h??ng
var roiOptions = new RoiOptions 
{ 
    FaceConfidence = 0.7f,
    FacePaddingRatio = new ExpandRatio(
        top: 0.25f,     // 25% padding trên
        bottom: 0.15f,  // 15% padding d??i
        left: 0.20f,    // 20% padding trái
        right: 0.20f    // 20% padding ph?i
    ),
    FacesUnionAll = true,
    SaliencyPaddingRatio = new ExpandRatio(0.0f)  // ??ng nh?t 4 h??ng
};

// T?o ImageProcessor v?i options
using var processor = new ImageProcessor(roiOptions);

// Kh?i t?o mô hình (b?t ??ng b?)
await processor.InitFaceModelAsync();

// S? d?ng
using var image = new ImageData("photo.jpg");
var roi = processor.GetAttentionRoi(image, targetSize);
ImageProcessor.Crop(image, roi);
```

## Troubleshooting

### Không phát hi?n ???c khuôn m?t?
- Gi?m `face.confidence` xu?ng 0.4-0.5
- Ki?m tra xem ?nh có ch?a khuôn m?t rõ ràng không
- Th? v?i ?nh có ?? phân gi?i cao h?n

### ROI không nh? mong ??i?
- Th? ?i?u ch?nh padding theo t?ng h??ng riêng bi?t
- T?ng `face.padding_top` n?u b? c?t tóc
- T?ng `face.padding_bottom` n?u b? c?t vai/c?
- ?i?u ch?nh `face.padding_left`/`face.padding_right` cho composition t?t h?n
- Th? ??i `face.union_all` gi?a true/false
- Xem xét dùng `Prominent` mode n?u ?nh không có ng??i

### ROI b? l?ch m?t bên?
- S? d?ng padding b?t ??i x?ng ?? ?i?u ch?nh
- Ví d?: `face.padding_left: 0.30`, `face.padding_right: 0.10` ?? ??y subject sang ph?i

### Hi?u n?ng ch?m?
- Ch? ?? Attention ch?m h?n Center và Prominent do c?n nh?n di?n khuôn m?t
- Ch? dùng Attention cho ?nh có ng??i
- Mô hình ???c cache nên l?n ??u s? ch?m h?n các l?n sau
