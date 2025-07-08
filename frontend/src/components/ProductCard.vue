<template>
  <div class="product-card" :class="{ 'available': product.isAvailable, 'unavailable': !product.isAvailable }">
    <div class="product-header">
      <h3 class="product-name">{{ product.name }}</h3>
      <span class="product-price">{{ product.formattedPrice }}</span>
    </div>
    
    <div class="product-info">
      <div class="product-quantity">
        <span class="quantity-label">Stock:</span>
        <span 
          class="quantity-value" 
          :class="getQuantityClass(product.quantity)"
        >
          {{ product.quantity }}
        </span>
      </div>
      
      <div 
        class="status-indicator" 
        :class="{ 'status-available': product.isAvailable, 'status-unavailable': !product.isAvailable }"
      >
        {{ product.isAvailable ? 'Disponible' : 'Agotado' }}
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ProductCard',
  props: {
    product: {
      type: Object,
      required: true
    }
  },
  methods: {
    getQuantityClass(quantity) {
      if (quantity === 0) return 'out-of-stock'
      if (quantity <= 3) return 'low-stock'
      return ''
    }
  }
}
</script>
